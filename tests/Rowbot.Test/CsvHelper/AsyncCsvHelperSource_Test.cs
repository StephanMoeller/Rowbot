﻿using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;

namespace Rowbot.CsvHelper.Test
{
    public class AsyncCsvHelperSource_Test
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EmptyInput_ExpectException(bool readFirstLineAsHeaders)
        {
            var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            var stream = new MemoryStream();
            stream.Write(utf8.GetBytes(""));
            stream.Position = 0;

            var source = new AsyncCsvHelperSource(stream, new CsvConfiguration(CultureInfo.InvariantCulture), readFirstLineAsHeaders: readFirstLineAsHeaders);
            _ = await Assert.ThrowsAsync<ReaderException>(() => source.InitAndGetColumnsAsync());
        }

        [Fact]
        public async Task  NonEmptyInput_ReadFirstLineAsHeaders()
        {
            var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            var stream = new MemoryStream();
            stream.Write(utf8.GetBytes("Col1;Col2;Col3\r\nData1A;Data1B;Data1C\r\nData2A;Data2B;Data2C"));
            stream.Position = 0;

            var source = new AsyncCsvHelperSource(stream, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" }, readFirstLineAsHeaders: true);
            var columns = await source.InitAndGetColumnsAsync();
            Assert.Equal(3, columns.Length);
            Assert.Equal("Col1", columns[0].Name);
            Assert.Equal(typeof(string), columns[0].ValueType);

            Assert.Equal("Col2", columns[1].Name);
            Assert.Equal(typeof(string), columns[1].ValueType);

            Assert.Equal("Col3", columns[2].Name);
            Assert.Equal(typeof(string), columns[2].ValueType);

            var lines = new List<object[]>();
            var buffer = new object[3];
            while (await source.ReadRowAsync(buffer))
            {
                lines.Add(buffer);
                buffer = new object[3];
            }

            Assert.Equal(2, lines.Count);
            Assert.Equal("Data1A", lines[0][0]);
            Assert.Equal("Data1B", lines[0][1]);
            Assert.Equal("Data1C", lines[0][2]);

            Assert.Equal("Data2A", lines[1][0]);
            Assert.Equal("Data2B", lines[1][1]);
            Assert.Equal("Data2C", lines[1][2]);
        }

        [Fact]
        public async Task  NonEmptyInput_ReadFirstLineAsData()
        {
            var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            var stream = new MemoryStream();
            stream.Write(utf8.GetBytes("Data1A;Data1B;Data1C\r\nData2A;Data2B;Data2C\r\nData3A;Data3B;Data3C"));
            stream.Position = 0;

            var source = new AsyncCsvHelperSource(stream, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" }, readFirstLineAsHeaders: false);
            var columns = await source.InitAndGetColumnsAsync();
            Assert.Equal(3, columns.Length);
            Assert.Equal("Column1", columns[0].Name);
            Assert.Equal(typeof(string), columns[0].ValueType);

            Assert.Equal("Column2", columns[1].Name);
            Assert.Equal(typeof(string), columns[1].ValueType);

            Assert.Equal("Column3", columns[2].Name);
            Assert.Equal(typeof(string), columns[2].ValueType);

            var lines = new List<object[]>();
            var buffer = new object[3];
            while (await source.ReadRowAsync(buffer))
            {
                lines.Add(buffer);
                buffer = new object[3];
            }

            Assert.Equal(3, lines.Count);
            Assert.Equal("Data1A", lines[0][0]);
            Assert.Equal("Data1B", lines[0][1]);
            Assert.Equal("Data1C", lines[0][2]);

            Assert.Equal("Data2A", lines[1][0]);
            Assert.Equal("Data2B", lines[1][1]);
            Assert.Equal("Data2C", lines[1][2]);

            Assert.Equal("Data3A", lines[2][0]);
            Assert.Equal("Data3B", lines[2][1]);
            Assert.Equal("Data3C", lines[2][2]);
        }
    }
}
