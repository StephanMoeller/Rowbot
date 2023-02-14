using CsvHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rowbot.CsvHelper
{
    public class CsvHelperTarget : RowTarget
    {
        private readonly CsvWriter _csvWriter;

        public CsvHelperTarget(CsvWriter csvWriter)
        {
            _csvWriter = csvWriter;
        }

        public override void Dispose()
        {
            _csvWriter.Dispose();
        }

        protected override void OnComplete()
        {
            _csvWriter.Flush();
        }

        protected override void OnInit(ColumnInfo[] columns)
        {
            _csvWriter.WriteRecords(columns.Select(c => c.Name).ToArray());
        }

        protected override void OnWriteRow(object[] values)
        {
            _csvWriter.WriteRecords(values);
        }
    }
}
