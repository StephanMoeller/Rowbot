using Rowbot.Targets;
using System.Data;
using System.Net.Http.Headers;

namespace Rowbot.Test.Core.Targets
{
    public class DataTableTarget_Test
    {
        [Fact]
        public void WriteColumnsAndRows_NoRows_ExpectEmptyTable_Test()
        {
            var table = new DataTable();
            var target = new DataTableTarget(table);

            target.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            target.Complete();
            
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
        public void WriteColumnsAndRows_Test()
        {
            var table = new DataTable();
            var target = new DataTableTarget(table);

            target.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            target.WriteRow(new object?[] { "Hello there æå 1", -12.45m, 42 });
            target.WriteRow(new object?[] { "Hello there æå 2", 10, null });
            target.WriteRow(new object?[] { "Hello there æå 3", null, int.MinValue });
            target.WriteRow(new object?[] { null, "123", int.MaxValue });

            target.Complete();
            
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
        public void WriteColumnsAndRows_IncompatibaleTypeInRow_ExpectExceptionFromDataTableRethrown_Test()
        {
            var table = new DataTable();
            var target = new DataTableTarget(table);

            target.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            target.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });
            Assert.Throws<ArgumentException>(() => target.WriteRow(new object?[] { "Hello there æå 2", "This string cannot be converted into an int", null }));
        }

        [Fact]
        public void Call_GetResultBeforeCallingCompleted_ExpectException_Test()
        {
            var table = new DataTable();
            var target = new DataTableTarget(table);

            target.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            target.WriteRow(new object?[] { "Hello there æå 1", -12.45m, 42 });
            target.WriteRow(new object?[] { "Hello there æå 2", 10, null });
            target.WriteRow(new object?[] { "Hello there æå 3", null, int.MinValue });
            target.WriteRow(new object?[] { null, decimal.MaxValue, int.MaxValue });

            Assert.Throws<InvalidOperationException>(() => target.GetResult());
        }

        private void AssertRowsValues(DataRow row, params object[] expectedValues)
        {
            for (var i = 0; i < expectedValues.Length; i++)
            {
                Assert.Equal(expectedValues[i], row[i]);
            }
        }
    }
}
