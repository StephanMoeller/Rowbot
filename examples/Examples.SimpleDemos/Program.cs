using Dapper;
using Rowbot.Core.Execution;
using Rowbot.Core.Targets;
using System.Data;
using System.Data.SqlClient;

namespace Examples.SimpleDemos
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var myObjects = Enumerable.Range(0, 10).Select(num => new { });
            DataTable myDataTable = new DataTable();
            string myConnectionString = "...";

            // Objects => CSV (Space complexity: O(1))
            new RowbotExecutorBuilder()
                .FromObjects(myObjects)
                .ToCsv(filepath: "c:\\temp\\rowbot.csv", config: new CsvConfig() { Delimiter = ';', Quote = '\'' }, writeHeaders: true)
                .Execute();

            // Objects => Excel (Space complexity: O(1))
            new RowbotExecutorBuilder()
                .FromObjects(myObjects)
                .ToExcel(filepath: "c:\\temp\\rowbot.xlsx", sheetName: "MySheet", writeHeaders: true)
                .Execute();

            // DataTable => CSV (Space complexity: O(1))
            new RowbotExecutorBuilder()
                .FromDataTable(myDataTable)
                .ToCsv(filepath: "c:\\temp\\rowbot.csv", config: new CsvConfig() { Delimiter = ';', Quote = '\'' }, writeHeaders: true)
                .Execute();

            // DataTable => Excel (Space complexity: O(1))
            new RowbotExecutorBuilder()
                .FromDataTable(myDataTable)
                .ToExcel(filepath: "c:\\temp\\rowbot.xlsx", sheetName: "MySheet", writeHeaders: true)
                .Execute();

            // Database => CSV (Space complexity: O(1))
            // using Dapper;
            using (var conn = new SqlConnection(myConnectionString))
            {
                conn.Open();
                var dataReader = conn.ExecuteReader("SELECT * FROM Orders WHERE CustomerId = @customerId", new { customerId = 123 });

                new RowbotExecutorBuilder()
                    .FromDataReader(dataReader)
                    .ToCsv(filepath: "c:\\temp\\rowbot.csv", config: new CsvConfig() { Delimiter = ';', Quote = '\'' }, writeHeaders: true)
                    .Execute();
            }

            // Database => Excel (Space complexity: O(1))
            // using Dapper;
            using (var conn = new SqlConnection(myConnectionString))
            {
                conn.Open();
                var dataReader = conn.ExecuteReader("SELECT * FROM Orders WHERE CustomerId = @customerId", new { customerId = 123 });

                new RowbotExecutorBuilder()
                    .FromDataReader(dataReader)
                    .ToExcel(filepath: "c:\\temp\\rowbot.xlsx", sheetName: "MySheet", writeHeaders: true)
                    .Execute();
            }

            // DataTable => List of objects (Space complexity: O(1))
            new RowbotExecutorBuilder()
                .FromDataTable(myDataTable)
                .ToObjects<Customer>()
                .Execute(objects =>
                {
                    foreach (var customer in objects)
                    {
                        // Do something with the customer here
                    }
                });
        }

        public class Customer
        {
            public long Id { get; set; }
            public string? Name { get; set; }
        }
    }
}