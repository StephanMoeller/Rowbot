using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;
using Xunit;

namespace Rowbot.CsvHelper.Test
{
    public class CsvHelperTarget_Test
    {
        [Fact]
        public void WriteColumnsAndRows_NoRows_ExpectEmptyTable_Test()
        {
            using (var ms = new MemoryStream())
            using (var target = new CsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'', NewLine = "\r\n" }, writeHeaders: true, leaveOpen: true))
            {
                target.Init(new ColumnInfo[]{
                        new ColumnInfo(name: "Col1", valueType: typeof(string)),
                        new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                        new ColumnInfo(name: "Col ��� 3; and this is an inline quote: ' awdawd", valueType: typeof(int)),
                    });

                target.Complete();

                var result = Encoding.UTF8.GetString(ms.ToArray());
                Assert.Equal("Col1;Col2;'Col ��� 3; and this is an inline quote: \"' awdawd'", result);
            }
        }

        [Fact]
        public void WriteColumnsAndRows_WithHeaders_Test()
        {
            using (var ms = new MemoryStream())
            using (var target = new CsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'', NewLine = "\r\n" }, writeHeaders: true, leaveOpen: true))
            {

                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "Col1", valueType: typeof(string)),
                    new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                    new ColumnInfo(name: "Col3", valueType: typeof(object)),
                });

                target.WriteRow(new object?[] { "Hello there �� 1", -12.45m, "hi", "there" }); // non-strings
                target.WriteRow(new object?[] { "Hello there �� 2", 12.45m, null, null }); // null values
                target.WriteRow(new object?[] { "Hello there �� 3", null, "This text has a ' in it.", "And this has a \r CR" }); // Quotes are double encoded (escaped)

                target.Complete();

                var result = Encoding.UTF8.GetString(ms.ToArray());
                Assert.Equal("Col1;Col2;Col3\r\nHello there �� 1;-12.45;hi;there\r\nHello there �� 2;12.45;;\r\nHello there �� 3;;'This text has a \"' in it.';And this has a \r CR", result);
            }
        }

        [Fact]
        public void WriteColumnsAndRows_WithoutHeaders_Test()
        {
            using (var ms = new MemoryStream())
            using (var target = new CsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'', NewLine = "\r\n" }, writeHeaders: false, leaveOpen: true))
            {

                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "Col1", valueType: typeof(string)),
                    new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                    new ColumnInfo(name: "Col3", valueType: typeof(object)),
                });

                target.WriteRow(new object?[] { "Hello there �� 1", -12.45m, "hi", "there" }); // non-strings
                target.WriteRow(new object?[] { "Hello there �� 2", 12.45m, null, null }); // null values
                target.WriteRow(new object?[] { "Hello there �� 3", null, "This text has a ' in it.", "And this has a \r CR" }); // Quotes are double encoded (escaped)

                target.Complete();

                var result = Encoding.UTF8.GetString(ms.ToArray());
                Assert.Equal("Hello there �� 1;-12.45;hi;there\r\nHello there �� 2;12.45;;\r\nHello there �� 3;;'This text has a \"' in it.';And this has a \r CR", result);
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
                CsvHelperTarget target;
                if (leaveOpen == null)
                {
                    // Rely on default behaviour
                    target = new CsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'', NewLine = "\r\n" }, writeHeaders: false);
                }
                else
                {
                    target = new CsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'', NewLine = "\r\n" }, writeHeaders: false, leaveOpen: leaveOpen.Value);
                }

                using (target)
                {
                    // Ensure init and writeWrote are allowed and stream not disposed as of yet
                    target.Init(new ColumnInfo[]{
                        new ColumnInfo(name: "Col1", valueType: typeof(string)),
                        new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                        new ColumnInfo(name: "Col3", valueType: typeof(object)),
                    });

                    target.WriteRow(new object?[] { "Hello there �� 1", -12.45m, "hi", "there" }); // non-strings

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