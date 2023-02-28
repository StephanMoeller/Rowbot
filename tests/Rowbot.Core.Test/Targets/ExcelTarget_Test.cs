﻿using ClosedXML.Excel;
using Rowbot.Core.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Core.Test.Targets
{
    public class ExcelTarget_Test
    {
        [Theory]
        [InlineData("A", 1)]
        [InlineData("B", 2)]
        [InlineData("C", 3)]
        [InlineData("D", 4)]
        [InlineData("E", 5)]
        [InlineData("F", 6)]
        [InlineData("G", 7)]

        [InlineData("H", 8)]
        [InlineData("I", 9)]
        [InlineData("J", 10)]
        [InlineData("K", 11)]
        [InlineData("L", 12)]
        [InlineData("M", 13)]
        [InlineData("N", 14)]

        [InlineData("O", 15)]
        [InlineData("P", 16)]
        [InlineData("Q", 17)]
        [InlineData("R", 18)]
        [InlineData("S", 19)]
        [InlineData("T", 20)]
        [InlineData("U", 21)]

        [InlineData("V", 22)]
        [InlineData("W", 23)]
        [InlineData("X", 24)]
        [InlineData("Y", 25)]
        [InlineData("Z", 26)]
        [InlineData("AA",27)]
        [InlineData("AB", 28)]
        [InlineData("AC", 29)]
        [InlineData("AY", 26 + 26 - 1)]
        [InlineData("AZ", 26 + 26)]
        [InlineData("BA", 26 + 26 + 1)]
        [InlineData("BB", 26 + 26 + 2)]
        [InlineData("BC", 26 + 26 + 3)]
        [InlineData("CA", 26 + 26 + 26 + 1)]
        public void GetColumnName(string expectedName, int oneBasedIndex)
        {
            var name = ExcelTarget.GetColumnName(oneBasedIndex);
            Assert.Equal(expectedName, name);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EmptyColumns_ExpectNoRowsAsExcelDoesNotHaveTheConceptOfEmptySheetWithRows_Test(bool writeHeaders)
        {
            using (var ms = new MemoryStream())
            using (var target = new ExcelTarget(ms, writeHeaders: writeHeaders, leaveOpen: true)) // Leave open to be able to read from the stream after completion
            {
                target.Init(new ColumnInfo[] { });

                target.WriteRow(new object[0]);
                target.WriteRow(new object[0]);

                target.Complete();

                EnsureExcelContent(ms, expectedSheetName: "My unittest æøå δ sheet", expectedRowValues: new object[0][]);
            }
        }



        public const int Call_Complete = 1;
        public const int Call_Dispose = 2;

        [Theory]
        [InlineData(Call_Complete, true, true)]
        [InlineData(Call_Complete, false, false)]
        [InlineData(Call_Complete, null, false)]
        [InlineData(Call_Dispose, true, true)]
        [InlineData(Call_Dispose, false, false)]
        [InlineData(Call_Dispose, null, false)]
        public void LeaveOpen_EnsureDisposingStreamAccordingToLeaveOpenValue_Test(int whatToCall, bool? leaveOpen, bool expectToLeaveOpen)
        {
            using (var ms = new MemoryStream())
            {
                ExcelTarget target;
                if (leaveOpen == null)
                {
                    // Rely on default behaviour
                    target = new ExcelTarget(ms, writeHeaders: true);
                }
                else
                {
                    target = new ExcelTarget(ms, writeHeaders: true, leaveOpen: leaveOpen.Value);
                }

                using (target)
                {
                    // Ensure init and writeWrote are allowed and stream not disposed as of yet
                    target.Init(new ColumnInfo[]{
                        new ColumnInfo(name: "Col1", valueType: typeof(string)),
                        new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                        new ColumnInfo(name: "Col3", valueType: typeof(object)),
                    });

                    target.WriteRow(new object?[] { "Hello there æå 1", -12.45m, "hi" }); // non-strings

                    // Now call complete
                    if (whatToCall == Call_Complete)
                    {
                        target.Complete();
                    }
                    else
                    {
                        target.Dispose();
                    }

                    // And ensure memory stream is disposed only if leaveOpen was false
                    if (expectToLeaveOpen)
                    {
                        ms.WriteByte(1);
                    }
                    else
                    {
                        Assert.Throws<ObjectDisposedException>(() => ms.WriteByte(1));
                    }
                }
            }

        }


        private void EnsureExcelContent(MemoryStream ms, string expectedSheetName, object?[][] expectedRowValues)
        {
            ms.Position = 0;

            // Code copied from https://www.aspsnippets.com/Articles/Read-and-Import-Excel-data-to-DataTable-using-ClosedXml-in-ASPNet-with-C-and-VBNet.aspx

            //Open the Excel file using ClosedXML.
            using (XLWorkbook workBook = new XLWorkbook(ms))
            {
                //Read the first Sheet from Excel file.
                Assert.Equal(1, workBook.Worksheets.Count); // Ensure only one sheet in thing to open

                IXLWorksheet workSheet = workBook.Worksheet(1);
                Assert.Equal(expectedSheetName, workSheet.Name);

                //Loop through the Worksheet rows.
                for (var rowIndex = 0; rowIndex < expectedRowValues.Length; rowIndex++)
                {
                    var expectedRow = expectedRowValues[rowIndex];
                    for (var columnIndex = 0; columnIndex < expectedRow.Length; columnIndex++)
                    {
                        var value = workSheet.Cell(row: rowIndex + 1, column: columnIndex + 1).Value;
                        Assert.Equal(CreateCellValue(expectedRow[columnIndex]), (object)value);
                    }
                }
            }
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
            if (value is String text)
                return text;
            if (value is XLError error)
                return error;
            if (value is DateTime date)
                return date;
            if (value is DateTimeOffset dateOfs)
                return dateOfs.DateTime;
            if (value is TimeSpan timeSpan)
                return timeSpan;
            return value.ToString(); // Other things, like chars ect are just turned to string
        }
    }
}
