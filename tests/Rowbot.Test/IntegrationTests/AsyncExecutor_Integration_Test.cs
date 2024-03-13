using Rowbot.Execution;
using System.Data;

namespace Rowbot.Test.IntegrationTests
{
    public class AsyncExecutor_Integration_Test
    {
        [Fact]
        public async Task ExecuteAsync_AnonymousObjectsToDataTable_Test()
        {
            var objects = AsyncEnumerable.Range(0, 3).Select(num => new { MyNum = num, MyName = $"My name is {num}" });
            var table = new DataTable();
            await new RowbotAsyncExecutorBuilder()
                .FromObjects(objects)
                .ToDataTable(table)
                .ExecuteAsync();

            // Assert columns
            Assert.Equal(2, table.Columns.Count);

            Assert.Equal("MyNum", table.Columns[0].ColumnName);
            Assert.Equal(typeof(int), table.Columns[0].DataType);

            Assert.Equal("MyName", table.Columns[1].ColumnName);
            Assert.Equal(typeof(string), table.Columns[1].DataType);

            // Assert rows
            Assert.Equal(3, table.Rows.Count);
            Assert.Equal(0, table.Rows[0]["MyNum"]);
            Assert.Equal("My name is 0", table.Rows[0]["MyName"]);

            Assert.Equal(1, table.Rows[1]["MyNum"]);
            Assert.Equal("My name is 1", table.Rows[1]["MyName"]);

            Assert.Equal(2, table.Rows[2]["MyNum"]);
            Assert.Equal("My name is 2", table.Rows[2]["MyName"]);
        }
    }
}
