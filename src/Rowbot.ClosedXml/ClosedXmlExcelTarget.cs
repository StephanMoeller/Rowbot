using ClosedXML.Excel;
using System;
using System.IO;

namespace Rowbot.ClosedXml
{
    public class ClosedXmlExcelTarget : RowTarget
    {
        private Stream _stream;
        private readonly string _sheetName;
        private readonly bool _writeHeaders;
        private readonly bool _leaveOpen;
        private XLWorkbook _workbook;
        private IXLWorksheet _worksheet;
        private int _rowIndex;
        public ClosedXmlExcelTarget(Stream outputStream, string sheetName, bool writeHeaders, bool leaveOpen = false)
        {
            _stream = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
            _sheetName = sheetName ?? throw new ArgumentNullException(nameof(sheetName));
            _writeHeaders = writeHeaders;
            _leaveOpen = leaveOpen;
        }

        public override void Dispose()
        {
            _workbook?.Dispose();
            if (!_leaveOpen)
            {
                _stream?.Dispose();
            }
        }

        protected override void OnComplete()
        {
            _workbook.SaveAs(_stream);
            _workbook.Dispose();
            if (!_leaveOpen)
            {
                _stream?.Dispose();
            }
        }

        protected override void OnInit(ColumnInfo[] columns)
        {
            _workbook = new XLWorkbook();
            _worksheet = _workbook.Worksheets.Add(_sheetName);
            
            if (_writeHeaders)
            {
                for (var i = 0; i < columns.Length; i++)
                {
                    var column = columns[i];
                    _worksheet.Cell(1, (i+1)).Value = column.Name;
                }
                _rowIndex = 2;
            }else{
                _rowIndex = 1;
            }

        }

        protected override void OnWriteRow(object[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                _worksheet.Cell(row: _rowIndex, column: i + 1).Value = values[i]?.ToString();
            }
            _rowIndex++;
        }
    }
}
