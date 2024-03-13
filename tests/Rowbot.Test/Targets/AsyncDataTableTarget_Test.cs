using Rowbot.Targets;
using System.Data;

namespace Rowbot.Test.Targets
{
    public class AsyncDataTableTarget_Test
    {
        [Fact]
        public async Task WriteColumnsAndRows_NoRows_ExpectEmptyTable_Test()
        {
            var table = new DataTable();
            var target = new AsyncDataTableTarget(table);

            await target.InitAsync(new ColumnInfo[]
            {
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            await target.CompleteAsync();

            Assert.NotNull(table);
            Assert.Equal(3, table.Columns.Count);

            Assert.Equal("Col1", table.Columns[0].ColumnName);
            Assert.Equal(typeof(string), table.Columns[0].DataType);

            Assert.Equal("Col2", table.Columns[1].ColumnName);
            Assert.Equal(typeof(decimal), table.Columns[1].DataType);

            Assert.Equal("Col æøå 3", table.Columns[2].ColumnName);
            Assert.Equal(typeof(int), table.Columns[2].DataType);

            Assert.Equal(0, table.Rows.Count);
        }

        [Fact]
        public async Task WriteColumnsAndRows_Test()
        {
            var table = new DataTable();
            var target = new AsyncDataTableTarget(table);

            await target.InitAsync(new ColumnInfo[]
            {
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            await target.WriteRowAsync(new object?[] { "Hello there æå 1", -12.45m, 42 });
            await target.WriteRowAsync(new object?[] { "Hello there æå 2", 10, null });
            await target.WriteRowAsync(new object?[] { "Hello there æå 3", null, int.MinValue });
            await target.WriteRowAsync(new object?[] { null, "123", int.MaxValue });

            await target.CompleteAsync();

            Assert.NotNull(table);
            Assert.Equal(3, table.Columns.Count);

            Assert.Equal("Col1", table.Columns[0].ColumnName);
            Assert.Equal(typeof(string), table.Columns[0].DataType);

            Assert.Equal("Col2", table.Columns[1].ColumnName);
            Assert.Equal(typeof(decimal), table.Columns[1].DataType);

            Assert.Equal("Col æøå 3", table.Columns[2].ColumnName);
            Assert.Equal(typeof(int), table.Columns[2].DataType);

            Assert.Equal(4, table.Rows.Count);

            AssertRowsValues(table.Rows[0], "Hello there æå 1", -12.45m, 42);
            AssertRowsValues(table.Rows[1], "Hello there æå 2", 10m, DBNull.Value);
            AssertRowsValues(table.Rows[2], "Hello there æå 3", DBNull.Value, int.MinValue);
            AssertRowsValues(table.Rows[3], DBNull.Value, 123m, int.MaxValue);
        }

        [Fact]
        public async Task WriteColumnsAndRows_IncompatibleTypeInRow_ExpectExceptionFromDataTableRethrown_Test()
        {
            var table = new DataTable();
            var target = new AsyncDataTableTarget(table);

            await target.InitAsync(new ColumnInfo[]
            {
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            await target.WriteRowAsync(new object[] { "Hello there æå 1", -12.45m, 42 });
            _ = await Assert.ThrowsAsync<ArgumentException>(() => target.WriteRowAsync(new object?[]
                { "Hello there æå 2", "This string cannot be converted into an int", null }));
        }

        private static void AssertRowsValues(DataRow row, params object[] expectedValues)
        {
            for (var i = 0; i < expectedValues.Length; i++)
            {
                Assert.Equal(expectedValues[i], row[i]);
            }
        }
    }
}