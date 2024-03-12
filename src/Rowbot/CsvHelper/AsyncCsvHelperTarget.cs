using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Rowbot.CsvHelper
{
    public sealed class AsyncCsvHelperTarget : IAsyncRowTarget, IDisposable
    {
        private readonly CsvWriter _csvWriter;
        private readonly bool _writeHeaders;
        private int _unflushedRowCount = 0;
        private bool _firstWrite = true;
        
        public AsyncCsvHelperTarget(Stream stream, CsvConfiguration configuration, bool writeHeaders = true, bool leaveOpen = false) : this(new CsvWriter(new StreamWriter(stream), configuration, leaveOpen: leaveOpen))
        {
            _writeHeaders = writeHeaders;
        }

        public AsyncCsvHelperTarget(CsvWriter csvWriter)
        {
            _csvWriter = csvWriter;
        }

        public void Dispose()
        {
            _csvWriter?.Dispose();
        }

        public async Task CompleteAsync()
        {
            await FlushAsync();
            _csvWriter?.Dispose();
        }

        public Task InitAsync(ColumnInfo[] columns)
        {
            if (_writeHeaders)
            {
                for (var i = 0; i < columns.Length; i++)
                {
                    _csvWriter.WriteField(columns[i].Name);
                }
                _firstWrite = false;
            }

            return Task.CompletedTask;
        }

        public async Task WriteRowAsync(object[] values)
        {
            if (!_firstWrite)
            {
                await _csvWriter.NextRecordAsync();
            }
            for (var i = 0; i < values.Length; i++)
            {
                _csvWriter.WriteField(values[i]);
            }
            _unflushedRowCount++;
            _firstWrite = false;
            await FlushIfNeeded();
        }

        private async Task FlushIfNeeded()
        {
            if (_unflushedRowCount > 1000)
            {
                await FlushAsync();
            }
        }

        private async Task FlushAsync()
        {
            await _csvWriter.FlushAsync();
            _unflushedRowCount = 0;
        }
    }
}
