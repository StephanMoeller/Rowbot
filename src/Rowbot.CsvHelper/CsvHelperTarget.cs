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
        private int _unflushedRowCount = 0;
        private bool _firstWrite = true;
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
            Flush();
        }

        protected override void OnInit(ColumnInfo[] columns)
        {
            if(_writeHeaders)
            {
                for (var i = 0; i < columns.Length; i++)
                {
                    _csvWriter.WriteField(columns[i].Name);
                }
                _firstWrite = false;
            }
        }

        protected override void OnWriteRow(object[] values)
        {
            if(!_firstWrite)
            {
                _csvWriter.NextRecord();
            }
            for (var i = 0; i < values.Length; i++)
            {
                _csvWriter.WriteField(values[i]);
            }
            _unflushedRowCount++;
            _firstWrite = false;
            FlushIfNeeded();
        }

        private void FlushIfNeeded()
        {
            if (_unflushedRowCount > 1000)
            {
                Flush();
            }
        }

        private void Flush()
        {
            _csvWriter.Flush();
            _unflushedRowCount = 0;
        }
    }
}
