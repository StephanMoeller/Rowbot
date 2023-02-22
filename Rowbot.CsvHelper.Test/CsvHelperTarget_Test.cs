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
            using (var target = new CsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'', NewLine = "\r\n" }, writeHeaders: true, leaveOpen: true))
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
                Assert.Equal("Col1;Col2;Col3\r\nHello there זו 1;-12.45;hi;there\r\nHello there זו 2;12.45;;\r\nHello there זו 3;;'This text has a \"' in it.';And this has a \r CR", result);
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

                target.WriteRow(new object?[] { "Hello there זו 1", -12.45m, "hi", "there" }); // non-strings
                target.WriteRow(new object?[] { "Hello there זו 2", 12.45m, null, null }); // null values
                target.WriteRow(new object?[] { "Hello there זו 3", null, "This text has a ' in it.", "And this has a \r CR" }); // Quotes are double encoded (escaped)

                target.Complete();

                var result = Encoding.UTF8.GetString(ms.ToArray());
                Assert.Equal("Hello there זו 1;-12.45;hi;there\r\nHello there זו 2;12.45;;\r\nHello there זו 3;;'This text has a \"' in it.';And this has a \r CR", result);
            }
        }


        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void LeaveOpen_EnsureCompleteCallWillDisposeStreamAccordingToLeaveOpenValue_Test(bool leaveOpen)
        {
            using (var ms = new MemoryStream())
            using (var target = new CsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'', NewLine = "\r\n" }, writeHeaders: false, leaveOpen: leaveOpen))
            {
                // Ensure init and writeWrote are allowed and stream not disposed as of yet
                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "Col1", valueType: typeof(string)),
                    new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                    new ColumnInfo(name: "Col3", valueType: typeof(object)),
                });
                target.WriteRow(new object?[] { "Hello there זו 1", -12.45m, "hi", "there" }); // non-strings

                // Now call complete
                target.Complete();

                // And ensure memory stream is disposed only if leaveOpen was false
                if (leaveOpen)
                {
                    ms.WriteByte(1);
                }
                else
                {
                    Assert.Throws<ObjectDisposedException>(() => ms.WriteByte(1));
                }
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void LeaveOpen_EnsureDisposeCallWillDisposeStreamAccordingToLeaveOpenValue_Test(bool leaveOpen)
        {
            using (var ms = new MemoryStream())
            using (var target = new CsvHelperTarget(ms, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", Quote = '\'', NewLine = "\r\n" }, writeHeaders: false, leaveOpen: leaveOpen))
            {
                // Ensure init and writeWrote are allowed and stream not disposed as of yet
                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "Col1", valueType: typeof(string)),
                    new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                    new ColumnInfo(name: "Col3", valueType: typeof(object)),
                });
                target.WriteRow(new object?[] { "Hello there זו 1", -12.45m, "hi", "there" }); // non-strings

                // Now call complete
                target.Dispose();

                // And ensure memory stream is disposed only if leaveOpen was false
                if (leaveOpen)
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

    public class DummyStream : Stream
    {
        public bool CloseCalled { get; private set; } = false;
        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => 1000;

        public override long Position { get; set; }

        public override void Flush()
        {
            
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 1;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return 1;
        }

        public override void SetLength(long value)
        {
            
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            
        }

        public override void Close()
        {
            base.Close();
            CloseCalled = true;
        }
    }
}