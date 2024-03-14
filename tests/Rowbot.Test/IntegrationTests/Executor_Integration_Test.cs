using Rowbot.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Test.IntegrationTests
{
    public class Executor_Integration_Test
    {
        [Fact]
        public void Execute_AnonymousObjectsToDataTable_Test()
        {
            var objects = Enumerable.Range(0, 3).Select(num => new {MyNum = num, MyName = $"My name is {num}"});
            var table = new DataTable();
            new RowbotExecutorBuilder()
                .FromObjects(objects)
                .ToDataTable(table)
                .Execute();

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

        [Fact]
        public void ExecuteAndIterate_AnonymousObjectsToDataTable_Test()
        {
            var objects = Enumerable.Range(0, 3).Select(num => new {MyNum = num * 10, MyName = $"My name is {num}"});
            var itemsList = new List<UnitTestDto>();

            new RowbotExecutorBuilder()
                .FromObjects(objects)
                .ToObjects<UnitTestDto>(items => { itemsList.AddRange(items); })
                .Execute();

            // Assert columns
            Assert.Equal(3, itemsList.Count);

            // Assert rows
            Assert.Equal(0, itemsList[0].MyNum);
            Assert.Equal("My name is 0", itemsList[0].MyName);

            Assert.Equal(10, itemsList[1].MyNum);
            Assert.Equal("My name is 1", itemsList[1].MyName);

            Assert.Equal(20, itemsList[2].MyNum);
            Assert.Equal("My name is 2", itemsList[2].MyName);
        }

        private class UnitTestDto
        {
            public int MyNum { get; set; }
            public string MyName { get; set; }
        }
    }
}