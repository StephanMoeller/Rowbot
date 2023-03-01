using Rowbot.Core.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Rowbot.Core.Targets
{
    public class ExcelTarget : IRowTarget
    {
        private readonly string _sheetName;
        private readonly bool _writeHeaders;
        private ColumnInfo[] _columns;
        private ZipArchive _zipArchive;
        private Stream _sheetStream;
        private UTF8Encoding _utf8;
        private byte[] _buffer = new byte[1024];
        private int _bufferIndex = 0;
        private static readonly string _columnChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string[] _excelColumnNames = null;
        private int _rowIndex = 0;
        private string _cache_minMaxColString;
        public ExcelTarget(Stream outputStream, string sheetName, bool writeHeaders, bool leaveOpen = false)
        {
            if (string.IsNullOrEmpty(sheetName))
            {
                throw new ArgumentException($"'{nameof(sheetName)}' cannot be null or empty.", nameof(sheetName));
            }

            _zipArchive = new ZipArchive(outputStream, ZipArchiveMode.Create, leaveOpen: leaveOpen);
            _sheetName = sheetName;
            _writeHeaders = writeHeaders;
            _utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        }

        public void Dispose()
        {
            _zipArchive?.Dispose();
            _sheetStream?.Dispose();
        }

        public static string GetColumnName(int oneBasedColumnIndex)
        {
            StringBuilder sb = new StringBuilder();
            var remainder = oneBasedColumnIndex;
            while (remainder > 0)
            {
                var nextIndex = (remainder - 1) % 26;
                sb.Insert(0, _columnChars[nextIndex]);
                remainder -= (nextIndex + 1);
                remainder /= 26;
            }
            return sb.ToString();
        }

        public void Init(ColumnInfo[] columns)
        {
            _columns = columns;
            WriteStaticFilesToArchive();

            var entry = _zipArchive.CreateEntry("xl/worksheets/sheet1.xml");
            _sheetStream = entry.Open();

            WriteSheetStartToSheetStream();
            _excelColumnNames = new string[columns.Length];

            for (int i = 0; i < columns.Length; i++)
            {
                _excelColumnNames[i] = GetColumnName(oneBasedColumnIndex: i + 1);
            };

            _rowIndex = 1;

            int minCol = 1;
            int maxCol = _columns.Length;
            _cache_minMaxColString = $"{minCol}:{maxCol}";

            if (_writeHeaders)
            {
                WriteRow(columns.Select(c => c.Name).Cast<object>().ToArray());
            }
        }

        public void Complete()
        {
            WriteSheetEndToSheetStream();
            Flush();
            _sheetStream.Close();
            _zipArchive.Dispose();

        }

        private CultureInfo _numberFormatter = new CultureInfo("en-US");
        public void WriteRow(object[] values)
        {
            WriteSheetBytes(@"<row r=""", _rowIndex.ToString(), @""" spans=""", _cache_minMaxColString, @""" x14ac:dyDescent=""0.25"">");

            // Blank:
            // null

            // Boolean:
            // bool

            // Number:
            // SByte, Byte, Int16, UInt16, Int32, UInt32, Int64, UInt64
            // Single, Double, Decimal

            // Text
            // String
            // Fallback .ToString()

            // DateTime
            // DateTime, DateTimeOffset

            // TimeSpan
            // TimeSpan

            for (var i = 0; i < values.Length; i++)
            {
                var value = values[i];
                if (value == null)
                {
                    WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @"""></c>");
                }
                else
                {
                    switch (value)
                    {
                        case bool boolVal:
                            if (boolVal)
                            {
                                WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @""" t=""b""><v>1</v></c>");
                            }
                            else
                            {
                                WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @""" t=""b""><v>0</v></c>");
                            }
                            break;
                        case SByte val:
                            WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @"""><v>", val.ToString(), "</v></c>");
                            break;
                        case Byte val:
                            WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @"""><v>", val.ToString(), "</v></c>");
                            break;
                        case Int16 val:
                            WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @"""><v>", val.ToString(), "</v></c>");
                            break;
                        case UInt16 val:
                            WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @"""><v>", val.ToString(), "</v></c>");
                            break;
                        case Int32 val:
                            WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @"""><v>", val.ToString(), "</v></c>");
                            break;
                        case UInt32 val:
                            WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @"""><v>", val.ToString(), "</v></c>");
                            break;
                        case Int64 val:
                            WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @"""><v>", val.ToString(), "</v></c>");
                            break;
                        case UInt64 val:
                            WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @"""><v>", val.ToString(), "</v></c>");
                            break;
                        case Single val:
                            WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @"""><v>", val.ToString(_numberFormatter), "</v></c>");
                            break;
                        case Double val:
                            WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @"""><v>", val.ToString(_numberFormatter), "</v></c>");
                            break;
                        case Decimal val:
                            WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @"""><v>", val.ToString(_numberFormatter), "</v></c>");
                            break;
                        case String str:
                            WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @""" t=""inlineStr""><is><t>", Escape(str), "</t></is></c>");
                            break;
                        case DateTime val:
                            WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @""" t=""inlineStr""><is><t>", val.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"), "</t></is></c>");
                            break;
                        default:
                            WriteSheetBytes(@"<c r=""", _excelColumnNames[i], _rowIndex.ToString(), @""" t=""inlineStr""><is><t>", Escape(value.ToString()), "</t></is></c>");
                            break;
                    }
                };
            }

            WriteSheetBytes("</row>");

            _rowIndex++;
            if (_rowIndex % 1000 == 0)
                FlushSheetStreamIfNeeded();
        }


        private static string Escape(string text)
        {
            return text.Replace("<", "&lt;").Replace(">", "&gt;");
        }


        private void WriteSheetStartToSheetStream()
        {
            WriteSheetBytes(@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" mc:Ignorable=""x14ac xr xr2 xr3"" xmlns:x14ac=""http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac"" xmlns:xr=""http://schemas.microsoft.com/office/spreadsheetml/2014/revision"" xmlns:xr2=""http://schemas.microsoft.com/office/spreadsheetml/2015/revision2"" xmlns:xr3=""http://schemas.microsoft.com/office/spreadsheetml/2016/revision3"" xr:uid=""{5D83E5F9-5829-419F-8F80-08EADF6B0FB7}"">
    <dimension ref=""A1""/>
    <sheetViews>
        <sheetView tabSelected=""1"" workbookViewId=""0"">
            <selection activeCell=""B2"" sqref=""B2""/>
        </sheetView>
    </sheetViews>
    <sheetFormatPr defaultRowHeight=""15"" x14ac:dyDescent=""0.25""/>
    <sheetData>");
        }

        private void WriteSheetEndToSheetStream()
        {
            WriteSheetBytes(@"</sheetData>
    <pageMargins left=""0.7"" right=""0.7"" top=""0.75"" bottom=""0.75"" header=""0.3"" footer=""0.3""/>
</worksheet>");
        }

        private void WriteStaticFilesToArchive()
        {
            WriteStatic(path: "[Content_Types].xml", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
    <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
    <Default Extension=""xml"" ContentType=""application/xml""/>
    <Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>
    <Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>
</Types>");

            WriteStatic(path: "_rels/.rels", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
    <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>
</Relationships>");

            WriteStatic(path: "xl/_rels/workbook.xml.rels", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
    <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"" Target=""worksheets/sheet1.xml""/>
</Relationships>");

            var escapedSheetname = _sheetName.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
            WriteStatic(path: "xl/workbook.xml", @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" mc:Ignorable=""x15 xr xr6 xr10 xr2"" xmlns:x15=""http://schemas.microsoft.com/office/spreadsheetml/2010/11/main"" xmlns:xr=""http://schemas.microsoft.com/office/spreadsheetml/2014/revision"" xmlns:xr6=""http://schemas.microsoft.com/office/spreadsheetml/2016/revision6"" xmlns:xr10=""http://schemas.microsoft.com/office/spreadsheetml/2016/revision10"" xmlns:xr2=""http://schemas.microsoft.com/office/spreadsheetml/2015/revision2"">
    <fileVersion appName=""xl"" lastEdited=""7"" lowestEdited=""7"" rupBuild=""26026""/>
    <workbookPr defaultThemeVersion=""166925""/>
    <mc:AlternateContent xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"">
        <mc:Choice Requires=""x15"">
            <x15ac:absPath url=""C:\temp\Book1 - Copy.xlsx\"" xmlns:x15ac=""http://schemas.microsoft.com/office/spreadsheetml/2010/11/ac""/>
        </mc:Choice>
    </mc:AlternateContent>
    <xr:revisionPtr revIDLastSave=""0"" documentId=""13_ncr:1_{3617F10C-6F32-4868-9EEC-1D33189EC57E}"" xr6:coauthVersionLast=""47"" xr6:coauthVersionMax=""47"" xr10:uidLastSave=""{00000000-0000-0000-0000-000000000000}""/>
    <bookViews>
        <workbookView xWindow=""10680"" yWindow=""0"" windowWidth=""31365"" windowHeight=""21000"" xr2:uid=""{4C7AFD97-58A3-4092-BBCA-D87C64F9BACE}""/>
    </bookViews>
    <sheets>
        <sheet name=""" + escapedSheetname + @""" sheetId=""1"" r:id=""rId1""/>
    </sheets>
    <calcPr calcId=""191029""/>
    <extLst>
        <ext uri=""{140A7094-0E35-4892-8432-C4D2E57EDEB5}"" xmlns:x15=""http://schemas.microsoft.com/office/spreadsheetml/2010/11/main"">
            <x15:workbookPr chartTrackingRefBase=""1""/>
        </ext>
        <ext uri=""{B58B0392-4F1F-4190-BB64-5DF3571DCE5F}"" xmlns:xcalcf=""http://schemas.microsoft.com/office/spreadsheetml/2018/calcfeatures"">
            <xcalcf:calcFeatures>
                <xcalcf:feature name=""microsoft.com:RD""/>
                <xcalcf:feature name=""microsoft.com:Single""/>
                <xcalcf:feature name=""microsoft.com:FV""/>
                <xcalcf:feature name=""microsoft.com:CNMTM""/>
                <xcalcf:feature name=""microsoft.com:LET_WF""/>
                <xcalcf:feature name=""microsoft.com:LAMBDA_WF""/>
                <xcalcf:feature name=""microsoft.com:ARRAYTEXT_WF""/>
            </xcalcf:calcFeatures>
        </ext>
    </extLst>
</workbook>");
        }

        private void WriteStatic(string path, string completeContent)
        {
            var entry = _zipArchive.CreateEntry(path);
            using (var entryStream = entry.Open())
            {
                var buffer = _utf8.GetBytes(completeContent);
                entryStream.Write(buffer, 0, buffer.Length);
                entryStream.Flush();
            }
        }

        private Dictionary<string, byte[]> _stringToBytesCache = new Dictionary<string, byte[]>();

        private void WriteSheetBytes(params string[] strings)
        {
            foreach (var str in strings)
            {
                // Grow buffer
                if (_bufferIndex + str.Length * 4 > _buffer.Length - 1)
                {
                    int newSize = Math.Max(_bufferIndex + str.Length * 4, _buffer.Length * 2);
                    Array.Resize(ref _buffer, newSize);
                }

                var bytesWritten = _utf8.GetBytes(s: str, charIndex: 0, charCount: str.Length, bytes: _buffer, byteIndex: _bufferIndex);
                _bufferIndex += bytesWritten;
            }
        }

        private void FlushSheetStreamIfNeeded()
        {
            if (_bufferIndex > 8_000_000)
            {
                Flush();
            }
        }

        private void Flush()
        {
            _sheetStream.Write(_buffer, 0, _bufferIndex);
            _sheetStream.Flush();
            _bufferIndex = 0;
        }
    }
}
