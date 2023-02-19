using Rowbot.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rowbot.Core.Targets
{
    public class CsvTarget : RowTarget
    {
        private readonly Stream _outputStream;
        private readonly Encoding _encoding;
        private readonly byte _delimiter;
        private readonly NumberToStringBytesSerializer _intSerializer;
        private readonly CsvEscapingHelper _escapingHelper;
        private byte[] _newLineBytes;
        private Func<object, byte[], int, int>[] _serializersByPropertyIndex;

        private Dictionary<Type, Func<object, byte[], int, int>> _serializersByType;
        private Func<object, byte[], int, int> _fallbackSerializer;
        private bool _initialized = false;
        private bool _completed = false;
        public CsvTarget(Stream outputStream, CsvConfig csvConfig)
        {
            _outputStream = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
            _encoding = csvConfig.Encoding;
            _delimiter = _encoding.GetBytes(new[] { csvConfig.Delimiter }).Single();
            _newLineBytes = _encoding.GetBytes(csvConfig.Newline);
            _intSerializer = new NumberToStringBytesSerializer(_encoding);
            _escapingHelper = new CsvEscapingHelper(delimiter: csvConfig.Delimiter, quote: csvConfig.Quote, encoding: csvConfig.Encoding);
            _serializersByType = new Dictionary<Type, Func<object, byte[], int, int>>
            {
                { typeof(byte), (value, buffer, offset) => _intSerializer.SerializeToStringToBytes(val: (byte)value, buffer: buffer, offset: offset) },
                { typeof(byte?), (value, buffer, offset) => _intSerializer.SerializeToStringToBytes(val: (byte) value, buffer: buffer, offset: offset) },
                { typeof(short), (value, buffer, offset) => _intSerializer.SerializeToStringToBytes(val: (short) value, buffer: buffer, offset: offset) },
                { typeof(short?), (value, buffer, offset) => _intSerializer.SerializeToStringToBytes(val: (short)value, buffer: buffer, offset: offset) },
                { typeof(int), (value, buffer, offset) => _intSerializer.SerializeToStringToBytes(val: (int)value, buffer: buffer, offset: offset) },
                { typeof(int?), (value, buffer, offset) => _intSerializer.SerializeToStringToBytes(val: (int)value, buffer: buffer, offset: offset) },
                { typeof(long), (value, buffer, offset) => _intSerializer.SerializeToStringToBytes(val: (long)value, buffer: buffer, offset: offset) },
                { typeof(long?), (value, buffer, offset) => _intSerializer.SerializeToStringToBytes(val: (long)value, buffer: buffer, offset: offset) },
                { typeof(string), (value, buffer, offset) => _escapingHelper.WriteEscapedString(value: (string)value, buffer: buffer, offset: offset) },
            };
            _fallbackSerializer = (value, buffer, offset) => _escapingHelper.WriteEscapedString(value: value.ToString(), buffer: buffer, offset: offset);
        }

        public override void Dispose()
        {
            // Nothing to dispose
        }


        private byte[] _buffer = new byte[1_000_000];
        private int _bufferIndex = 0;
        protected override void OnInit(ColumnInfo[] columns)
        {
            if (_initialized)
                throw new InvalidOperationException("Init already called");
            _initialized = true;


            _serializersByPropertyIndex = new Func<object, byte[], int, int>[columns.Length];
            for (var i = 0; i < columns.Length; i++)
            {
                var column = columns[i];
                if (i > 0)
                {
                    _buffer[_bufferIndex++] = _delimiter;
                }
                var bytes = _escapingHelper.WriteEscapedString(column.Name, _buffer, _bufferIndex);
                _bufferIndex += bytes;

                // Create serializers pr property index for effective reference during row-writes
                _serializersByPropertyIndex[i] = _serializersByType.ContainsKey(column.ValueType) ? _serializersByType[column.ValueType] : _fallbackSerializer;
            }

            FlushIfNeeded();
        }

        private void FlushIfNeeded()
        {
            if (_bufferIndex > 500_000)
            {
                Flush();
            }
        }


        protected override void OnComplete()
        {
            if (!_initialized)
                throw new InvalidOperationException("Init must be called before Complete()");
            if (_completed)
                throw new InvalidOperationException("Already completed");
            _completed = true;

            Flush();
        }


        private void Flush()
        {
            if (_bufferIndex > 0)
            {
                _outputStream.Write(buffer: _buffer, offset: 0, _bufferIndex);
                _bufferIndex = 0;
            }
        }


        protected override void OnWriteRow(object[] values)
        {

            // Write newline
            for (var i = 0; i < _newLineBytes.Length; i++)
            {
                _buffer[_bufferIndex++] = _newLineBytes[i];
            }

            for (var i = 0; i < values.Length; i++)
            {
                // Write column delimiter
                if (i > 0)
                {
                    _buffer[_bufferIndex++] = _delimiter;
                }

                var value = values[i];
                if (value != null)
                {
                    var serializer = _serializersByPropertyIndex[i];
                    int byteCount = serializer(value, _buffer, _bufferIndex);
                    _bufferIndex += byteCount;
                }
            }

            FlushIfNeeded();
        }
    }

    public class CsvConfig
    {
        public char Delimiter { get; set; }
        public char Quote { get; set; }
        public string Newline { get; set; }
        public Encoding Encoding { get; set; }

        public CsvConfig()
        {
            Quote = '"';
            Delimiter = ';';
            Newline = "\r\n";
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        }
    }

    internal class CsvEscapingHelper
    {
        private readonly char _delimiter;
        private readonly char _quote;
        private readonly Encoding _encoding;
        private char[] _charBuffer = new char[10];
        public CsvEscapingHelper(char delimiter, char quote, Encoding encoding)
        {
            _delimiter = delimiter;
            _quote = quote;
            _encoding = encoding;
        }

        public int WriteEscapedString(string value, byte[] buffer, int offset)
        {
            // Grow buffer if needed
            var requiredBufferSize = value.Length * 2 + 2;// Every char in string could be a quote, hence every char could be 2 chars in output as i would need to be escaped. And then 2 extra slots could be used for start and end quotes.
            if (requiredBufferSize >= _charBuffer.Length)
            {
                GrowBuffers(minimumCharSize: requiredBufferSize);
            }

            // value => char buffer
            int index = 1;
            bool sourroundingQuotesNeeded = false;

            for (var i = 0; i < value.Length; i++)
            {
                _charBuffer[index++] = value[i];
                if (value[i] == _quote || value[i] == _delimiter || value[i] == '\n' || value[i] == '\r')
                {
                    sourroundingQuotesNeeded = true;

                    // Add extra quote if this is a quote
                    if (value[i] == _quote)
                    {
                        _charBuffer[index++] = _quote;
                    }
                }

            }

            int startIndex = 1;
            int charCount = index - 1;
            if (sourroundingQuotesNeeded)
            {
                startIndex = 0;
                charCount += 2;
                _charBuffer[0] = _quote;
                _charBuffer[index] = _quote;
            }

            // Char buffer => byte buffer
            var byteCount = _encoding.GetBytes(chars: _charBuffer, charIndex: startIndex, charCount: charCount, bytes: buffer, byteIndex: offset);
            return byteCount;
        }

        private void GrowBuffers(int minimumCharSize)
        {
            _charBuffer = new char[Math.Max(minimumCharSize, _charBuffer.Length * 2)];
        }
    }
}
