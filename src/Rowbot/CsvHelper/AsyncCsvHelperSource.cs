﻿using CsvHelper;
using CsvHelper.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rowbot.CsvHelper
{
    public class AsyncCsvHelperSource : IAsyncRowSource
    {
        private readonly CsvReader _csvReader;
        private readonly bool _readFirstLineAsHeaders = false;
        private int _readCallCount = 0;
        
        public AsyncCsvHelperSource(Stream stream, CsvConfiguration configuration, bool readFirstLineAsHeaders) : this(new CsvReader(new StreamReader(stream), configuration), readFirstLineAsHeaders: readFirstLineAsHeaders)
        {            
        }

        public AsyncCsvHelperSource(CsvReader csvReader, bool readFirstLineAsHeaders)
        {
            _csvReader = csvReader;
            _readFirstLineAsHeaders = readFirstLineAsHeaders;
        }

        public void Dispose()
        {
            _csvReader.Dispose();
        }

        public Task CompleteAsync()
        {
            // Nothing to complete
            return Task.CompletedTask;
        }

        public async Task<ColumnInfo[]> InitAndGetColumnsAsync()
        {
            await _csvReader.ReadAsync();
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

        public async Task<bool> ReadRowAsync(object[] values)
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
                if (await _csvReader.ReadAsync())
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
