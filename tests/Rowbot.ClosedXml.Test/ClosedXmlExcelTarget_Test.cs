using ClosedXML.Excel;

namespace Rowbot.ClosedXml.Test
{
    public class ClosedXmlExcelTarget_Test
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EmptyColumns_ExpectNoRowsAsExcelDoesNotHaveTheConceptOfEmptySheetWithRows_Test(bool writeHeaders)
        {
            using (var ms = new MemoryStream())
            using (var target = new ClosedXmlExcelTarget(ms, sheetName: "My unittest æøå δ sheet", writeHeaders: writeHeaders, leaveOpen: true)) // Leave open to be able to read from the stream after completion
            {
                target.Init(new ColumnInfo[] { });

                target.WriteRow(new object[0]);
                target.WriteRow(new object[0]);

                target.Complete();

                EnsureExcelContent(ms, expectedSheetName: "My unittest æøå δ sheet", expectedRowValues: new object[0][]);
            }
        }

        [Fact]
        public void NoRows_WithWriteHeadersOff_ExpectNoRowsAsExcelDoesNotHaveTheConceptOfEmptySheetWithRows_Test()
        {
            using (var ms = new MemoryStream())
            using (var target = new ClosedXmlExcelTarget(ms, sheetName: "My unittest æøå δ sheet", writeHeaders: false, leaveOpen: true)) // Leave open to be able to read from the stream after completion
            {
                target.Init(new ColumnInfo[]{
                        new ColumnInfo(name: "Col1", valueType: typeof(string)),
                        new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                        new ColumnInfo(name: "Col æøå 3; and this is an inline quote: ' awdawd", valueType: typeof(int)),
                    });

                target.Complete();

                EnsureExcelContent(ms, expectedSheetName: "My unittest æøå δ sheet", expectedRowValues: new object[0][]);
            }
        }

        [Fact]
        public void NoRows_WithWriteHeadersOn_ExpectEmptyTableWithDefinedColumns_Test()
        {
            using (var ms = new MemoryStream())
            using (var target = new ClosedXmlExcelTarget(ms, sheetName: "My unittest æøå δ sheet", writeHeaders: true, leaveOpen: true)) // Leave open to be able to read from the stream after completion
            {
                target.Init(new ColumnInfo[]{
                        new ColumnInfo(name: "Col1", valueType: typeof(string)),
                        new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                        new ColumnInfo(name: "Col æøå 3; and this is an inline quote: ' awdawd", valueType: typeof(int)),
                    });

                target.Complete();

                EnsureExcelContent(ms, expectedSheetName: "My unittest æøå δ sheet", expectedRowValues: new object[][] {
                    new object[]{ "Col1", "Col2", "Col æøå 3; and this is an inline quote: ' awdawd" }
                });
            }
        }


        [Fact]
        public void WithRows_WithWriteHeadersOn_ExpectTableWithDefinedColumns_Test()
        {
            using (var ms = new MemoryStream())
            using (var target = new ClosedXmlExcelTarget(ms, sheetName: "My unittest æøå δ sheet", writeHeaders: true, leaveOpen: true)) // Leave open to be able to read from the stream after completion
            {
                target.Init(new ColumnInfo[]{
                        new ColumnInfo(name: "", valueType: typeof(string)),
                        new ColumnInfo(name: "Col æøå 3; and this is an inline quote: ' awdawd", valueType: typeof(decimal)),
                        new ColumnInfo(name: "", valueType: typeof(int)),
                    });

                target.WriteRow(new object?[] { "Hello ÆØÅ", -12.45m, 8_123_456 });
                target.WriteRow(new object?[] { null, null, null });
                target.WriteRow(new object?[] { "", "", "" });
                target.WriteRow(new object?[] { "Hello again \r\n with multi lines", null, 8_123_456 });

                target.Complete();

                //using (var fs = File.Create("C:\\temp\\output1.xlsx"))
                //{
                //    ms.Position = 0;
                //    ms.CopyTo(fs);
                //    fs.Close();
                //}

                EnsureExcelContent(ms, expectedSheetName: "My unittest æøå δ sheet", expectedRowValues: new object?[][] {
                    new object?[]{ null, "Col æøå 3; and this is an inline quote: ' awdawd", null },
                    new object?[] { "Hello ÆØÅ", -12.45m, 8123456 },
                    new object?[] { null, null, null },
                    new object?[] { null, null, null }, // Empty strings become blanks with the ClosedXml framework. (Or maybe in excel in general)
                    new object?[] { "Hello again \n with multi lines", null, 8123456 } // \r\n becomes \n when closed xml writes and reads
                });
            }
        }

        [Fact]
        public void WithRows_WithWriteHeadersOff_ExpectTableWithDefinedColumns_Test()
        {
            using (var ms = new MemoryStream())
            using (var target = new ClosedXmlExcelTarget(ms, sheetName: "My unittest æøå δ sheet", writeHeaders: false, leaveOpen: true)) // Leave open to be able to read from the stream after completion
            {
                target.Init(new ColumnInfo[]{
                        new ColumnInfo(name: "", valueType: typeof(string)),
                        new ColumnInfo(name: "Col æøå 3; and this is an inline quote: ' awdawd", valueType: typeof(decimal)),
                        new ColumnInfo(name: "", valueType: typeof(int)),
                    });

                target.WriteRow(new object?[] { "Hello ÆØÅ", -12.45m, 8_123_456 });
                target.WriteRow(new object?[] { null, null, null });
                target.WriteRow(new object?[] { "", "", "" });
                target.WriteRow(new object?[] { "Hello again \r\n with multi lines", null, 8_123_456 });

                target.Complete();

                //using (var fs = File.Create("C:\\temp\\output1.xlsx"))
                //{
                //    ms.Position = 0;
                //    ms.CopyTo(fs);
                //    fs.Close();
                //}

                EnsureExcelContent(ms, expectedSheetName: "My unittest æøå δ sheet", expectedRowValues: new object?[][] {
                    new object?[] { "Hello ÆØÅ", -12.45m, 8123456 },
                    new object?[] { null, null, null },
                    new object?[] { null, null, null }, // Empty strings become blanks with the ClosedXml framework. (Or maybe in excel in general)
                    new object?[] { "Hello again \n with multi lines", null, 8123456 } // \r\n becomes \n when closed xml writes and reads
                });
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
                ClosedXmlExcelTarget target;
                if (leaveOpen == null)
                {
                    // Rely on default behaviour
                    target = new ClosedXmlExcelTarget(ms, writeHeaders: true, sheetName: "SomeName");
                }
                else
                {
                    target = new ClosedXmlExcelTarget(ms, writeHeaders: true, sheetName: "SomeName", leaveOpen: leaveOpen.Value);
                }

                using (target)
                {
                    // Ensure init and writeWrote are allowed and stream not disposed as of yet
                    target.Init(new ColumnInfo[]{
                        new ColumnInfo(name: "Col1", valueType: typeof(string)),
                        new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                        new ColumnInfo(name: "Col3", valueType: typeof(object)),
                    });

                    target.WriteRow(new object?[] { "Hello there æå 1", -12.45m, "hi", "there" }); // non-strings

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
        private static XLCellValue CreateCellValue(object? value)
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