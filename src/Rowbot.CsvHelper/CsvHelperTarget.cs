using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rowbot.CsvHelper
{
    public class CsvHelperTarget : RowTarget
    {
        private readonly CsvWriter _csvWriter;
        private readonly bool _writeHeaders;

        public CsvHelperTarget(Stream stream, CsvConfiguration configuration, bool writeHeaders) : this(new CsvWriter(new StreamWriter(stream), configuration))
        {
            _writeHeaders = writeHeaders;
        }

        public CsvHelperTarget(CsvWriter csvWriter)
        {
            _csvWriter = csvWriter;
        }

        public override void Dispose()
        {
            _csvWriter?.Dispose();
        }

        protected override void OnComplete()
        {
            _csvWriter.Flush();
        }

        protected override void OnInit(ColumnInfo[] columns)
        {
            if(_writeHeaders)
            {
                for (var i = 0; i < columns.Length; i++)
                {
                    _csvWriter.WriteField(columns[i].Name);
                }
            }
        }

        protected override void OnWriteRow(object[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                _csvWriter.WriteField(values[i]);
            }
        }
    }
}
