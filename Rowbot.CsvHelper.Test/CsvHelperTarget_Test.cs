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
            using (var target = new CsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'' }, writeHeaders: true))
            {
                target.Init(new ColumnInfo[]{
                        new ColumnInfo(name: "Col1", valueType: typeof(string)),
                        new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                        new ColumnInfo(name: "Col זרו 3; and this is an inline quote: ' awdawd", valueType: typeof(int)),
                    });

                target.Complete();

                var result = Encoding.UTF8.GetString(ms.ToArray());
                Assert.Equal("Col1;Col2;'Col זרו 3; and this is an inline quote: \"' awdawd'", result);
            }
        }

        [Fact]
        public void WriteColumnsAndRows_WithHeaders_Test()
        {
            using (var ms = new MemoryStream())
            using (var target = new CsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'' }, writeHeaders: true))
            {

                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "Col1", valueType: typeof(string)),
                    new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                    new ColumnInfo(name: "Col3", valueType: typeof(object)),
                });

                target.WriteRow(new object?[] { "Hello there זו 1", -12.45m, "hi", "there" }); // non-strings
                target.WriteRow(new object?[] { "Hello there זו 2", 12.45m, null, null }); // null values
                target.WriteRow(new object?[] { "Hello there זו 3", null, "This text has a ' in it.", "And this has a \r CR" }); // Quotes are double encoded (escaped)

                target.Complete();

                var result = Encoding.UTF8.GetString(ms.ToArray());
                Assert.Equal("Col1;Col2;Col3;Hello there זו 1;-12.45;hi;there;Hello there זו 2;12.45;;;Hello there זו 3;;'This text has a \"' in it.';'And this has a \r CR'", result);
            }
        }

        [Fact]
        public void WriteColumnsAndRows_WithoutHeaders_Test()
        {
            using (var ms = new MemoryStream())
            using (var target = new CsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'' }, writeHeaders: false))
            {

                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "Col1", valueType: typeof(string)),
                    new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                    new ColumnInfo(name: "Col3", valueType: typeof(object)),
                });

                target.WriteRow(new object?[] { "Hello there זו 1", -12.45m, "hi", "there" }); // non-strings
                target.WriteRow(new object?[] { "Hello there זו 2", 12.45m, null, null }); // null values
                target.WriteRow(new object?[] { "Hello there זו 3", null, "This text has a ' in it.", "And this has a \r CR" }); // Quotes are double encoded (escaped)

                target.Complete();

                var result = Encoding.UTF8.GetString(ms.ToArray());
                Assert.Equal("Hello there זו 1;-12.45;hi;there;Hello there זו 2;12.45;;;Hello there זו 3;;'This text has a \"' in it.';'And this has a \r CR'", result);
            }
        }
    }
}