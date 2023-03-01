using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.IO;

namespace Rowbot.ClosedXml
{
    public class ClosedXmlExcelTarget : IRowTarget
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

        public void Dispose()
        {
            _workbook?.Dispose();
            if (!_leaveOpen)
            {
                _stream?.Dispose();
            }
        }

        public void Complete()
        {
            _workbook.SaveAs(_stream);
            _workbook.Dispose();
            if (!_leaveOpen)
            {
                _stream?.Dispose();
            }
        }

        public void Init(ColumnInfo[] columns)
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

        public void WriteRow(object[] values)
        {
            for (var i = 0; i < values.Length; i++)
            {
                _worksheet.Cell(row: _rowIndex, column: i + 1).Value = CreateCellValue(values[i]);
            }
            
            _rowIndex++;
        }

        // This method has been copied from 
        private static XLCellValue CreateCellValue(object value)
        {
            // This list of cases has been copied from the OpenXML source code directly (From inside XLWorksheet.cs)
            if (value == null) return Blank.Value;
            if (value is Blank blankValue)
                return blankValue;
            if (value is Boolean logical)
                return logical;
            if (value is SByte val_sbyte)
                return val_sbyte;
            if (value is Byte val_byte)
                return val_byte;
            if (value is Int16 val_int16)
                return val_int16;
            if (value is UInt16 val_uint16)
                return val_uint16;
            if (value is Int32 val_int32)
                return val_int32;
            if (value is UInt32 val_uint32)
                return val_uint32;
            if (value is Int64 val_int64)
                return val_int64;
            if (value is UInt64 val_uint64)
                return val_uint64;
            if (value is Single val_single)
                return val_single;
            if (value is Double val_double)
                return val_double;
            if (value is Decimal val_decimal)
                return val_decimal;
            if (value is String text )
                return  text;
            if (value is XLError error )
                return  error;
            if (value is DateTime date )
                return  date;
            if (value is DateTimeOffset dateOfs )
                return  dateOfs.DateTime;
            if (value is TimeSpan timeSpan )
                return  timeSpan;
            return value.ToString(); // Other things, like chars ect are just turned to string
        }
    }
}
