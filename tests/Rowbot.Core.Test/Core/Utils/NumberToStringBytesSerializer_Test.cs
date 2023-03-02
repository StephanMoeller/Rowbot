using Rowbot.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Test.Core.Utils
{
    public class NumberToStringBytesSerializer_Test
    {
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MinValue + 1)]
        [InlineData(-12345)]
        [InlineData(-1025)]
        [InlineData(-1024)]
        [InlineData(-1023)]
        [InlineData(-1)]
        [InlineData(-0)]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(30)]
        [InlineData(63)]
        [InlineData(99)]
        [InlineData(100)]
        [InlineData(101)]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1025)]
        [InlineData(12345)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MaxValue - 1)]
        public void SerializeToStringToBytes_Comparison_ByteArray(int val)
        {
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            var serializer = new NumberToStringBytesSerializer(encoding);

            var buffer1 = new byte[1000];
            var buffer2 = new byte[1000];

            int byteCount1 = encoding.GetBytes(val.ToString(), buffer1);
            int byteCount2 = serializer.SerializeToStringToBytes(val: val, buffer: buffer2, offset: 0);

            var str1 = string.Join(" ", buffer1.Take(byteCount1));
            var str2 = string.Join(" ", buffer2.Take(byteCount2));
            Assert.Equal(str1, str2);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MinValue + 1)]
        [InlineData(-12345)]
        [InlineData(-1025)]
        [InlineData(-1024)]
        [InlineData(-1023)]
        [InlineData(-1)]
        [InlineData(-0)]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(30)]
        [InlineData(63)]
        [InlineData(99)]
        [InlineData(100)]
        [InlineData(101)]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1025)]
        [InlineData(12345)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MaxValue - 1)]
        public void SerializeToStringToBytes_ByteArray_WithOffset(int val)
        {
            int offset = 7;
            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            var serializer = new NumberToStringBytesSerializer(encoding);

            var buffer1 = new byte[1000];
            var buffer2 = new byte[1000];

            int byteCount1 = encoding.GetBytes(val.ToString(), buffer1);
            int byteCount2 = serializer.SerializeToStringToBytes(val: val, buffer: buffer2, offset: offset);

            var str1 = string.Join(" ", buffer1.Take(byteCount1));
            var str2 = string.Join(" ", buffer2.Skip(offset).Take(byteCount2));
            for (var i = 0; i < offset; i++)
            {
                Assert.Equal(0, buffer2[i]); // Ensure before offset is untouched
            }
            Assert.Equal(str1, str2);
        }

        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MinValue + 1)]
        [InlineData(-12345)]
        [InlineData(-1025)]
        [InlineData(-1024)]
        [InlineData(-1023)]
        [InlineData(-1)]
        [InlineData(-0)]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(30)]
        [InlineData(63)]
        [InlineData(99)]
        [InlineData(100)]
        [InlineData(101)]
        [InlineData(1023)]
        [InlineData(1024)]
        [InlineData(1025)]
        [InlineData(12345)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MaxValue - 1)]
        public void SerializeToStringToBytes_OutputStream(int val)
        {

            var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            var serializer = new NumberToStringBytesSerializer(encoding);

            using (var ms1 = new MemoryStream())
            {
                serializer.SerializeToStringToBytes(val: val, outputStream: ms1);
                var buffer1 = ms1.ToArray();
                var buffer2 = encoding.GetBytes(val.ToString());

                var str1 = string.Join(" ", buffer1);
                var str2 = string.Join(" ", buffer2);
                Assert.Equal(str1, str2);
            }
        }

    }
}
