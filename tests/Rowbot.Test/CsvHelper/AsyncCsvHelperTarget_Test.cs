using CsvHelper.Configuration;
using System.Globalization;
using System.Text;

namespace Rowbot.CsvHelper.Test
{
    public class AsyncCsvHelperTarget_Test
    {
        [Fact]
        public async Task WriteColumnsAndRows_NoRows_ExpectEmptyTable_Test()
        {
            using (var ms = new MemoryStream())
            using (var target = new AsyncCsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'', NewLine = "\r\n" }, writeHeaders: true, leaveOpen: true))
            {
                await target.InitAsync(new ColumnInfo[]{
                        new ColumnInfo(name: "Col1", valueType: typeof(string)),
                        new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                        new ColumnInfo(name: "Col æøå 3; and this is an inline quote: ' awdawd", valueType: typeof(int)),
                    });

                await target.CompleteAsync();

                var result = Encoding.UTF8.GetString(ms.ToArray());
                Assert.Equal("Col1;Col2;'Col æøå 3; and this is an inline quote: \"' awdawd'", result);
            }
        }

        [Fact]
        public async Task  WriteColumnsAndRows_WithHeaders_Test()
        {
            using (var ms = new MemoryStream())
            using (var target = new AsyncCsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'', NewLine = "\r\n" }, writeHeaders: true, leaveOpen: true))
            {

                await target.InitAsync(new ColumnInfo[]{
                    new ColumnInfo(name: "Col1", valueType: typeof(string)),
                    new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                    new ColumnInfo(name: "Col3", valueType: typeof(object)),
                });

                await target.WriteRowAsync(new object?[] { "Hello there æå 1", -12.45m, "hi", "there" }); // non-strings
                await target.WriteRowAsync(new object?[] { "Hello there æå 2", 12.45m, null, null }); // null values
                await target.WriteRowAsync(new object?[] { "Hello there æå 3", null, "This text has a ' in it.", "And this has a \r CR" }); // Quotes are double encoded (escaped)

                await target.CompleteAsync();

                var result = Encoding.UTF8.GetString(ms.ToArray());
                Assert.Equal("Col1;Col2;Col3\r\nHello there æå 1;-12.45;hi;there\r\nHello there æå 2;12.45;;\r\nHello there æå 3;;'This text has a \"' in it.';And this has a \r CR", result);
            }
        }

        [Fact]
        public async Task  WriteColumnsAndRows_WithoutHeaders_Test()
        {
            using (var ms = new MemoryStream())
            using (var target = new AsyncCsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'', NewLine = "\r\n" }, writeHeaders: false, leaveOpen: true))
            {

                await target.InitAsync(new ColumnInfo[]{
                    new ColumnInfo(name: "Col1", valueType: typeof(string)),
                    new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                    new ColumnInfo(name: "Col3", valueType: typeof(object)),
                });

                await target.WriteRowAsync(new object?[] { "Hello there æå 1", -12.45m, "hi", "there" }); // non-strings
                await target.WriteRowAsync(new object?[] { "Hello there æå 2", 12.45m, null, null }); // null values
                await target.WriteRowAsync(new object?[] { "Hello there æå 3", null, "This text has a ' in it.", "And this has a \r CR" }); // Quotes are double encoded (escaped)

                await target.CompleteAsync();

                var result = Encoding.UTF8.GetString(ms.ToArray());
                Assert.Equal("Hello there æå 1;-12.45;hi;there\r\nHello there æå 2;12.45;;\r\nHello there æå 3;;'This text has a \"' in it.';And this has a \r CR", result);
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
        public async Task LeaveOpen_EnsureDisposingStreamAccordingToLeaveOpenValue_Test(int whatToCall, bool? leaveOpen, bool expectToLeaveOpen)
        {
            using (var ms = new MemoryStream())
            {
                AsyncCsvHelperTarget target;
                if (leaveOpen == null)
                {
                    // Rely on default behaviour
                    target = new AsyncCsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'', NewLine = "\r\n" }, writeHeaders: false);
                }
                else
                {
                    target = new AsyncCsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'', NewLine = "\r\n" }, writeHeaders: false, leaveOpen: leaveOpen.Value);
                }

                using (target)
                {
                    // Ensure init and writeWrote are allowed and stream not disposed as of yet
                    await target.InitAsync(new ColumnInfo[]{
                        new ColumnInfo(name: "Col1", valueType: typeof(string)),
                        new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                        new ColumnInfo(name: "Col3", valueType: typeof(object)),
                    });

                    await target.WriteRowAsync(new object?[] { "Hello there æå 1", -12.45m, "hi", "there" }); // non-strings

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
    }
}