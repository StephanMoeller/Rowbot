using ClosedXML.Excel;
using Rowbot.Targets;

namespace Rowbot.Test.Targets
{
    public class AsyncExcelTarget_Test
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
        [InlineData("AA", 27)]
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
            var name = AsyncExcelTarget.GetColumnName(oneBasedIndex);
            Assert.Equal(expectedName, name);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EmptyColumns_ExpectNoRowsAsExcelDoesNotHaveTheConceptOfEmptySheetWithRows_Test(
            bool writeHeaders)
        {
            using (var ms = new MemoryStream())
            using (var target = new AsyncExcelTarget(ms, sheetName: "My sheet < > \" and Φ╚",
                       writeHeaders: writeHeaders,
                       leaveOpen: true)) // Leave open to be able to read from the stream after completion
            {
                await target.InitAsync(Array.Empty<ColumnInfo>());

                await target.WriteRowAsync(Array.Empty<object>());
                await target.WriteRowAsync(Array.Empty<object>());

                await target.CompleteAsync();

                EnsureExcelContent(ms.ToArray(), expectedSheetName: "My sheet < > \" and Φ╚",
                    expectedRowValues: Array.Empty<XLCellValue[]>());
            }
        }

        [Theory]
        [InlineData("Me & My")]
        [InlineData("Me and \"My\"")]
        [InlineData("Me and 'My'")]
        [InlineData("Me and < My")]
        [InlineData("Me and > My")]
        public async Task SpecialCharactersInCell_Test(string data)
        {
            using (var ms = new MemoryStream())
            using (var target = new AsyncExcelTarget(ms, sheetName: "My sheet < > \" and Φ╚", writeHeaders: false,
                       leaveOpen: true)) // Leave open to be able to read from the stream after completion
            {
                await target.InitAsync(new ColumnInfo[]
                    { new ColumnInfo(name: "MyColumn123", valueType: typeof(string)) });

                await target.WriteRowAsync(new object[] { data });
                await target.WriteRowAsync(new object[] { data });

                await target.CompleteAsync();

                EnsureExcelContent(ms.ToArray(), expectedSheetName: "My sheet < > \" and Φ╚",
                    expectedRowValues: new XLCellValue[][] { new XLCellValue[] { data }, new XLCellValue[] { data } });
            }
        }

        [Theory]
        [InlineData("Me & My")]
        [InlineData("Me and \"My\"")]
        [InlineData("Me and 'My'")]
        [InlineData("Me and < My")]
        [InlineData("Me and > My")]
        public async Task SpecialCharactersInColumn_Test(string columnName)
        {
            using (var ms = new MemoryStream())
            using (var target = new AsyncExcelTarget(ms, sheetName: "My sheet < > \" and Φ╚", writeHeaders: true,
                       leaveOpen: true)) // Leave open to be able to read from the stream after completion
            {
                await target.InitAsync(new ColumnInfo[]
                    { new ColumnInfo(name: columnName, valueType: typeof(string)) });

                await target.WriteRowAsync(new object[] { "Hello1" });

                await target.CompleteAsync();

                EnsureExcelContent(ms.ToArray(), expectedSheetName: "My sheet < > \" and Φ╚",
                    expectedRowValues: new XLCellValue[][]
                        { new XLCellValue[] { columnName }, new XLCellValue[] { "Hello1" } });
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task SimpleTest_WithColumns(bool writeHeaders)
        {
            using (var ms = new MemoryStream())
            using (var target =
                   new AsyncExcelTarget(ms, sheetName: "My sheet", writeHeaders: writeHeaders,
                       leaveOpen: true)) // Leave open to be able to read from the stream after completion
            {
                await target.InitAsync(new ColumnInfo[]
                {
                    new ColumnInfo(name: "ColA", typeof(string)), new ColumnInfo(name: "Hey ÆØÅ <", typeof(string))
                });

                await target.WriteRowAsync(new object[] { "Hey \" and < and > in the text", "There" });
                await target.WriteRowAsync(new object[] { "There", "Over there" });

                await target.CompleteAsync();

                if (writeHeaders)
                {
                    EnsureExcelContent(ms.ToArray(), expectedSheetName: "My sheet",
                        expectedRowValues: new XLCellValue[][]
                        {
                            new XLCellValue[] { "ColA", "Hey ÆØÅ <" },
                            new XLCellValue[] { "Hey \" and < and > in the text", "There" },
                            new XLCellValue[] { "There", "Over there" }
                        });
                }
                else
                {
                    EnsureExcelContent(ms.ToArray(), expectedSheetName: "My sheet",
                        expectedRowValues: new XLCellValue[][]
                        {
                            new XLCellValue[] { "Hey \" and < and > in the text", "There" },
                            new XLCellValue[] { "There", "Over there" }
                        });
                }
            }
        }

        [Fact]
        public async Task DataTypeTesting_Null()
        {
            await RunTypeTestAsync<string>(null, expectedValue: Blank.Value);
            await RunTypeTestAsync<object>(null, expectedValue: Blank.Value);
            await RunTypeTestAsync<UnitTestCustomType>(null, expectedValue: Blank.Value);
        }

        [Theory]
        [InlineData("Hello < and > there")]
        [InlineData("Hello \"there\"")]
        public async Task DataTypeTesting_String(string value)
        {
            await RunTypeTestAsync(value: value, expectedValue: value);
        }

        [Fact]
        public async Task DataTypeTesting_NewLines()
        {
            //NOTE: This test uses ClosedXml to reload the excel data.
            // On load, ClosedXml will adjust any line breaks (\n or \r\n) and replace them with the value of Environment.Newline.
            // This causes different results when run on linux and windows machines.
            await RunTypeTestAsync(value: "New\nline", expectedValue: $"New{Environment.NewLine}line");
            await RunTypeTestAsync(value: "CR\r\nLF", expectedValue: $"CR{Environment.NewLine}LF");
        }

        [InlineData("Carriage\rreturn")]
        [InlineData("")]
        [InlineData("CR\r\nLF")]
#pragma warning disable xUnit1005 // Fact methods should not have test data
        [Fact]
        public async Task DataTypeTesting_IntFamily()
        {
            await RunTypeTestAsync<sbyte>(12, 12);
            await RunTypeTestAsync<byte>(12, 12);
            await RunTypeTestAsync<short>(12, 12);
            await RunTypeTestAsync<ushort>(12, 12);
            await RunTypeTestAsync(12, 12);
            await RunTypeTestAsync<uint>(12, 12);
            await RunTypeTestAsync<long>(12, 12);
            await RunTypeTestAsync<ulong>(12, 12);
        }
#pragma warning restore xUnit1005 // Fact methods should not have test data

        [Fact]
        public async Task DataTypeTesting_DecimalFamily()
        {
            await RunTypeTestAsync(12.3456m, 12.3456m);
            await RunTypeTestAsync(12.3456f, 12.3456f, numberCompareTolerance: 0.00001);
            await RunTypeTestAsync(12.3456, 12.3456, numberCompareTolerance: 0.00001);
        }

        [Fact(Skip = "To be fixed")]
        public async Task DataTypeTesting_DateTime()
        {
            await RunTypeTestAsync(value: new DateTime(2001, 02, 03, 04, 05, 06),
                expectedValue: new DateTime(2001, 02, 03, 04, 05, 06));

            throw new NotImplementedException("Why does this work??");
        }

        private async Task RunTypeTestAsync<T>(T? value, XLCellValue expectedValue, double numberCompareTolerance = 0.0)
        {
            using (var ms = new MemoryStream())
            using (var target =
                   new AsyncExcelTarget(ms, sheetName: "My sheet", writeHeaders: true,
                       leaveOpen: true)) // Leave open to be able to read from the stream after completion
            {
                await target.InitAsync(new ColumnInfo[]
                {
                    new ColumnInfo(name: "ColA", typeof(T))
                });

                await target.WriteRowAsync(new object?[] { value });

                await target.CompleteAsync();

                EnsureExcelContent(ms.ToArray(), expectedSheetName: "My sheet", expectedRowValues: new XLCellValue[][]
                {
                    new XLCellValue[] { "ColA" },
                    new XLCellValue[] { expectedValue }
                }, numberCompareTolerance: numberCompareTolerance);
            }
        }

        private const int Call_Complete = 1;
        private const int Call_Dispose = 2;

        [Theory]
        [InlineData(Call_Complete, true, true)]
        [InlineData(Call_Complete, false, false)]
        [InlineData(Call_Complete, null, false)]
        [InlineData(Call_Dispose, true, true)]
        [InlineData(Call_Dispose, false, false)]
        [InlineData(Call_Dispose, null, false)]
        public async Task LeaveOpen_EnsureDisposingStreamAccordingToLeaveOpenValue_Test(int whatToCall, bool? leaveOpen,
            bool expectToLeaveOpen)
        {
            using (var ms = new MemoryStream())
            {
                AsyncExcelTarget target;
                if (leaveOpen == null)
                {
                    // Rely on default behaviour
                    target = new AsyncExcelTarget(ms, sheetName: "My sheet", writeHeaders: true);
                }
                else
                {
                    target = new AsyncExcelTarget(ms, sheetName: "My sheet", writeHeaders: true,
                        leaveOpen: leaveOpen.Value);
                }

                using (target)
                {
                    // Ensure init and writeWrote are allowed and stream not disposed as of yet
                    await target.InitAsync(new ColumnInfo[]
                    {
                        new ColumnInfo(name: "Col1", valueType: typeof(string)),
                        new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                        new ColumnInfo(name: "Col3", valueType: typeof(object)),
                    });

                    await target.WriteRowAsync(new object?[] { "Hello there æå 1", -12.45m, "hi" }); // non-strings

                    // Now call complete
                    if (whatToCall == Call_Complete)
                    {
                        await target.CompleteAsync();
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

        private void EnsureExcelContent(byte[] excelData, string expectedSheetName, XLCellValue[][] expectedRowValues,
            double numberCompareTolerance = 0.0)
        {
            using (var ms = new MemoryStream(excelData))
            {
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

                            // If value type is double or single, compare with a little tolerance
                            var expectedValue = expectedRow[columnIndex];
                            if (expectedValue.IsNumber)
                            {
                                var expectedNumberValue = expectedValue.GetNumber();
                                Assert.Equal(expectedNumberValue, (double)value, tolerance: numberCompareTolerance);
                            }
                            else
                            {
                                Assert.Equal(expectedValue, value);
                            }
                        }

                        // Ensure no more columns than expected
                        var valueInNextCellToTheRight =
                            workSheet.Cell(row: rowIndex + 1, column: expectedRowValues.Length + 1).Value;
                        Assert.True(valueInNextCellToTheRight.IsBlank);
                    }

                    // Ensure no more rows than expected
                    var valueInNextRow = workSheet.Cell(row: expectedRowValues.Length + 1, column: 1).Value;
                    Assert.True(valueInNextRow.IsBlank);
                }
            }
        }

        private class UnitTestCustomType
        {
        }
    }
}