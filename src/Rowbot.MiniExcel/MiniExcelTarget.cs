using System;
using System.IO;
using MiniExcelLibs;

namespace Rowbot.MiniExcel
{
    public class MiniExcelTarget : RowTarget
    {
        private MemoryStream _outputStream;
        private string _sheetName;
        private bool _writeHeaders;
        private bool _leaveOpen;

        public MiniExcelTarget(MemoryStream outputStream, string sheetName, bool writeHeaders = true, bool leaveOpen = false)
        {
            _outputStream = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
            _sheetName = sheetName ?? throw new ArgumentNullException(nameof(sheetName));
            _writeHeaders = writeHeaders;
            _leaveOpen = leaveOpen;
        }

        public override void Dispose()
        {
            
        }

        protected override void OnComplete()
        {
            
        }

        protected override void OnInit(ColumnInfo[] columns)
        {
            
        }

        protected override void OnWriteRow(object[] values)
        {
            MiniExcelLibs.MiniExcel.Insert(_outputStream, values);
        }
    }
}
