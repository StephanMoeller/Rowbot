using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.IO;
using System.Linq;

namespace Rowbot.CsvHelper
{
    public class CsvHelperSource : RowSource
    {
        private readonly CsvReader _csvReader;
        private readonly bool _readFirstLineAsHeaders = false;
        private int _readCallCount = 0;
        public CsvHelperSource(Stream stream, CsvConfiguration configuration, bool readFirstLineAsHeaders) : this(new CsvReader(new StreamReader(stream), configuration))
        {
            _readFirstLineAsHeaders = readFirstLineAsHeaders;
        }

        public CsvHelperSource(CsvReader csvReader)
        {
            _csvReader = csvReader;
        }

        public override void Dispose()
        {
            _csvReader.Dispose();
        }

        protected override void OnComplete()
        {
            // Nothing to complete
        }

        protected override ColumnInfo[] OnInitAndGetColumns()
        {
            _csvReader.Read();
            _csvReader.ReadHeader();

            if (_readFirstLineAsHeaders)
            {
                var columns = _csvReader.HeaderRecord.Select(header => new ColumnInfo(name: header, valueType: typeof(string))).ToArray();
                return columns;
            }
            else
            {
                var columns = new ColumnInfo[_csvReader.HeaderRecord.Length];
                for (var i = 0; i < _csvReader.HeaderRecord.Length; i++)
                {
                    columns[i] = new ColumnInfo(name: $"Column{i + 1}", valueType: typeof(string));
                }
                return columns;
            }
        }

        protected override bool OnReadRow(object[] values)
        {
            _readCallCount++;
            if (_readCallCount == 1 && !_readFirstLineAsHeaders)
            {
                // First line should not be read as headers but as data. Copy from the headers line onto the values buffer
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = _csvReader.HeaderRecord[i];
                }
                return true;
            }
            else
            {
                if (_csvReader.Read())
                {

                    for (var i = 0; i < values.Length; i++)
                    {
                        values[i] = _csvReader.GetField(i);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }
    }
}
