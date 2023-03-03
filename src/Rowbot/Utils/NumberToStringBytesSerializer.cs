using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rowbot.Utils
{
    public class NumberToStringBytesSerializer
    {
        private readonly Encoding _encoding;
        private byte[] _digitToByteHash;
        private byte _minusSign;
        private byte[] minimum_int64_bytes;
        private byte[] _workingArray;
        public NumberToStringBytesSerializer(Encoding encoding)
        {
            _digitToByteHash = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }.Select(number => encoding.GetBytes(number.ToString()).Single()).ToArray();
            _minusSign = encoding.GetBytes("-").Single();
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            _workingArray = new byte[32];
        }

        public NumberToStringBytesSerializer(Encoding encoding, byte[] digitToByteHash, byte minusSign, byte[] minimum_int32_bytes, byte[] workingArray) : this(encoding)
        {
            _digitToByteHash = digitToByteHash;
            _minusSign = minusSign;
            minimum_int64_bytes = minimum_int32_bytes;
            _workingArray = workingArray;
        }

        public int SerializeToStringToBytes(long val, Stream outputStream)
        {
            int byteCount = SerializeNumberToArray(val, _workingArray, 0);
            outputStream.Write(buffer: _workingArray, offset: 0, count: byteCount);
            return byteCount;
        }

        public int SerializeToStringToBytes(long val, byte[] buffer, int offset)
        {
            int byteCount = SerializeNumberToArray(val: val, buffer: buffer, offset: offset);
            return byteCount;
        }

        private int SerializeNumberToArray(long val, byte[] buffer, int offset)
        {
            // Special case: long.MinValue cannot be * -1 as that would be 1 higher than long.MaxValue. The solution here is to just have a ready byte array for the case of this number in particular.
            if (val == long.MinValue)
            {
                if (minimum_int64_bytes == null)
                {
                    minimum_int64_bytes = _encoding.GetBytes(int.MinValue.ToString()).Reverse().ToArray();
                }
                for (var i = 0; i < minimum_int64_bytes.Length; i++)
                {
                    buffer[offset + i] = minimum_int64_bytes[i];
                }
                return minimum_int64_bytes.Length;
            }

            // Start serializing into working array in reversed order
            bool negativeInputNumber = false;
            if (val < 0)
            {
                negativeInputNumber = true;
                val *= -1;
            }

            int index = 0;
            var remainder = val;
            do
            {
                var lowestDigit = remainder % 10;
                buffer[offset + index] = _digitToByteHash[lowestDigit];
                index++;
                remainder /= 10;
            } while (remainder > 0);

            if (negativeInputNumber)
            {
                buffer[offset + index] = _minusSign;
                index++;
            }

            // Now reverse the final list of bytes
            for (var i = 0; i < index / 2; i++)
            {
                var temp = buffer[offset + index - i - 1];
                buffer[offset + index - i - 1] = buffer[offset + i];
                buffer[offset + i] = temp;
            }

            return index;
        }
    }
}
