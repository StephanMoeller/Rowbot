using Rowbot.Core.Targets;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Test.Targets
{
    public class CsvTarget_Test
    {
        [Fact]
        public void WriteColumnsAndRows_NoRows_ExpectEmptyTable_Test()
        {
            using (var ms = new MemoryStream())
            {
                var target = new CsvTarget(ms, new CsvConfig() { Delimiter = ';', Quote = '\'' });

                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "Col1", valueType: typeof(string)),
                    new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                    new ColumnInfo(name: "Col æøå 3; and this is an inline quote: ' awdawd", valueType: typeof(int)),
                });

                target.Complete();

                var result = Encoding.UTF8.GetString(ms.ToArray());
                Assert.Equal("Col1;Col2;'Col æøå 3; and this is an inline quote: '' awdawd'", result);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(null)]
        public void WriteColumnsAndRows_Test(bool? writeHeaders)
        {
            using (var ms = new MemoryStream())
            {
                var target = writeHeaders != null ? new CsvTarget(ms, new CsvConfig() { Delimiter = ';', Quote = '\'', Newline = "\r\n", NumberFormatter = new CultureInfo("da-DK") }, writeHeaders: writeHeaders.Value)
                                                    : new CsvTarget(ms, new CsvConfig() { Delimiter = ';', Quote = '\'', Newline = "\r\n", NumberFormatter = new CultureInfo("da-DK") });

                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "Col1", valueType: typeof(string)),
                    new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                    new ColumnInfo(name: "Col3", valueType: typeof(object)),
                    new ColumnInfo(name: "Col æøå 3; and this is an inline quote: ' awdawd", valueType: typeof(string)),
                });

                target.WriteRow(new object?[] { "Hello there æå 1", -12.45m, "hi", "there" }); // non-strings
                target.WriteRow(new object?[] { "Hello there æå 2", 12.45m, null, null }); // null values
                target.WriteRow(new object?[] { "Hello there æå 3", null, "This text has a ' in it.", "And this has a \r CR" }); // Quotes are double encoded (escaped)
                target.WriteRow(new object?[] { "Here is a \n LF", 0, "And this one has\r\n both CRLF", "This one has \r\n multiple \r \r occurrenced \n \n of each" }); // Quotes are double encoded (escaped)
                target.WriteRow(new object?[] { "This one contains the delimier as value; hence it should also be wrapped in quotes", 0, "Hey", "Hoo" });

                target.Complete();

                var result = Encoding.UTF8.GetString(ms.ToArray());
                if(writeHeaders != false) // Null is expected to be treated as true
                {
                    Assert.Equal("Col1;Col2;Col3;'Col æøå 3; and this is an inline quote: '' awdawd'\r\n"
                               + "Hello there æå 1;-12,45;hi;there\r\n"
                               + "Hello there æå 2;12,45;;\r\n"
                               + "Hello there æå 3;;'This text has a '' in it.';'And this has a \r CR'\r\n"
                               + "'Here is a \n LF';0;'And this one has\r\n both CRLF';'This one has \r\n multiple \r \r occurrenced \n \n of each'\r\n"
                               + "'This one contains the delimier as value; hence it should also be wrapped in quotes';0;Hey;Hoo", result);
                }else{
                    Assert.Equal("Hello there æå 1;-12,45;hi;there\r\n"
                                + "Hello there æå 2;12,45;;\r\n"
                                + "Hello there æå 3;;'This text has a '' in it.';'And this has a \r CR'\r\n"
                                + "'Here is a \n LF';0;'And this one has\r\n both CRLF';'This one has \r\n multiple \r \r occurrenced \n \n of each'\r\n"
                                + "'This one contains the delimier as value; hence it should also be wrapped in quotes';0;Hey;Hoo", result);
                }
                
            }
        }

        [Theory]
        [InlineData("en-US", "-12.4567")]
        [InlineData("da-DK", "-12,4567")]
        public void DecimalFormattingUsingInputCulture_Test(string cultureName, string expectedDecimalFormatting)
        {
            using (var ms = new MemoryStream())
            {
                var target = new CsvTarget(ms, new CsvConfig() { Delimiter = ';', Quote = '\'', Newline = "\r\n", NumberFormatter = new CultureInfo(cultureName) });

                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "Col1", valueType: typeof(decimal)),
                });

                target.WriteRow(new object?[] { -12.4567m }); // non-strings
                
                target.Complete();

                var result = Encoding.UTF8.GetString(ms.ToArray());
                Assert.Equal("Col1\r\n" + expectedDecimalFormatting, result);
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
                CsvTarget target;
                if (leaveOpen == null)
                {
                    // Rely on default behaviour
                    target = new CsvTarget(ms, new CsvConfig() { Delimiter = ';', Quote = '\'', Newline = "\r\n", NumberFormatter = new CultureInfo("da-DK") }, writeHeaders: true);
                }
                else
                {
                    target = new CsvTarget(ms, new CsvConfig() { Delimiter = ';', Quote = '\'', Newline = "\r\n", NumberFormatter = new CultureInfo("da-DK") }, writeHeaders: true, leaveOpen: leaveOpen.Value);
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

    }
}
