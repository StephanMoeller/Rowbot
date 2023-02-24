using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MiniExcelLibs;
using Rowbot.Sources;

namespace Rowbot.MiniExcel
{
    public class MiniExcelSource : DataReaderSource
    {
        public MiniExcelSource(Stream inputStream, bool readFirstRowAsHeader, MiniExcelLibs.IConfiguration configuration = null)
            : base(MiniExcelLibs.MiniExcel.GetReader(inputStream, useHeaderRow: readFirstRowAsHeader, excelType: ExcelType.XLSX, configuration: configuration))
        {
            
        }
    }
}
