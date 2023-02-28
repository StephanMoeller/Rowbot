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

        [Fact]
        public void WriteColumnsAndRows_Test()
        {
            using (var ms = new MemoryStream())
            {
                var target = new CsvTarget(ms, new CsvConfig() { Delimiter = ';', Quote = '\'', Newline = "\r\n", NumberFormatter = new CultureInfo("da-DK") });

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
                Assert.Equal("Col1;Col2;Col3;'Col æøå 3; and this is an inline quote: '' awdawd'\r\n"
                           + "Hello there æå 1;-12,45;hi;there\r\n"
                           + "Hello there æå 2;12,45;;\r\n"
                           + "Hello there æå 3;;'This text has a '' in it.';'And this has a \r CR'\r\n"
                           + "'Here is a \n LF';0;'And this one has\r\n both CRLF';'This one has \r\n multiple \r \r occurrenced \n \n of each'\r\n"
                           + "'This one contains the delimier as value; hence it should also be wrapped in quotes';0;Hey;Hoo", result);
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

    }
}
