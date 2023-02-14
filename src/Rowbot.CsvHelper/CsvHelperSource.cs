using CsvHelper;
using System;
using System.Globalization;
using System.Linq;

namespace Rowbot.CsvHelper
{
    public class CsvHelperSource : RowSource
    {
        private readonly CsvReader _csvReader;
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
            if (_csvReader.HeaderRecord == null)
            {
                throw new Exception("No headers detected in csv file");
            }

            var columns = _csvReader.HeaderRecord.Select(header => new ColumnInfo(name: header, valueType: typeof(string))).ToArray();
            return columns;
        }

        protected override bool OnReadRow(object[] values)
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
