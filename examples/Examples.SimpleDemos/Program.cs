using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using Rowbot.CsvHelper;
using Rowbot.Execution;
using Rowbot.Targets;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;

namespace Examples.SimpleDemos
{
    internal class Program
    {
        public class FooType
        {
        }

        static async Task Main(string[] args)
        {
            var myObjects = Enumerable.Range(0, 10).Select(num => new FooType());
            var myObjectsAsAsyncEnumerable = AsyncEnumerable.Range(0, 10).Select(num => new FooType());
            DataTable myDataTable = new DataTable();
            string myConnectionString = "...";

            // Objects => Excel
            // Sync version
            new RowbotExecutorBuilder()
                .FromObjects(myObjects)
                .ToExcel(filepath: "c:\\temp\\result.xlsx", sheetName: "Sheet1", writeHeaders: true)
                .Execute();

            // Async version
            await new RowbotAsyncExecutorBuilder()
                .FromObjects(myObjectsAsAsyncEnumerable)
                .ToExcel(filepath: "c:\\temp\\result.xlsx", sheetName: "Sheet1", writeHeaders: true)
                .ExecuteAsync();

            // Objects => Csv (Space complexity: O(1))
            // Sync version
            new RowbotExecutorBuilder()
                .FromObjects(myObjects)
                .ToCsvUsingCsvHelper(filepath: "c:\\temp\\output.csv", config: new CsvConfiguration(CultureInfo.InvariantCulture), writeHeaders: true)
                .Execute();

            // Async version
            await new RowbotAsyncExecutorBuilder()
                .FromObjects(myObjectsAsAsyncEnumerable)
                .ToCsvUsingCsvHelper(filepath: "c:\\temp\\output.csv", config: new CsvConfiguration(CultureInfo.InvariantCulture), writeHeaders: true)
                .ExecuteAsync();

            // DataTable => Excel (Space complexity: O(1))
            // Sync version
            new RowbotExecutorBuilder()
                .FromObjects(myObjects)
                .ToExcel(filepath: "c:\\temp\\output.xlsx", sheetName: "MySheet", writeHeaders: true)
                .Execute();

            // Async version
            await new RowbotAsyncExecutorBuilder()
                .FromObjects(myObjectsAsAsyncEnumerable)
                .ToExcel(filepath: "c:\\temp\\output.xlsx", sheetName: "MySheet", writeHeaders: true)
                .ExecuteAsync();

            // DataTable => Csv (Space complexity: O(1))
            // Sync version
            new RowbotExecutorBuilder()
                .FromDataTable(myDataTable)
                .ToCsvUsingCsvHelper(filepath: "c:\\temp\\output.csv", config: new CsvConfiguration(CultureInfo.InvariantCulture), writeHeaders: true)
                .Execute();

            // Async version
            await new RowbotAsyncExecutorBuilder()
                .FromDataTable(myDataTable)
                .ToCsvUsingCsvHelper(filepath: "c:\\temp\\output.csv", config: new CsvConfiguration(CultureInfo.InvariantCulture), writeHeaders: true)
                .ExecuteAsync();

            // Database => Excel (Space complexity: O(1))
            // Sync version
            // using Dapper;
            using (var conn = new SqlConnection(myConnectionString))
            {
                conn.Open();
                var dataReader = conn.ExecuteReader("SELECT * FROM Orders WHERE CustomerId = @customerId", new {customerId = 123});

                new RowbotExecutorBuilder()
                    .FromDataReader(dataReader)
                    .ToExcel(filepath: "c:\\temp\\output.xlsx", sheetName: "MySheet", writeHeaders: true)
                    .Execute();
            }

            // Async version
            using (var conn = new SqlConnection(myConnectionString))
            {
                await conn.OpenAsync();
                var dataReader = conn.ExecuteReader("SELECT * FROM Orders WHERE CustomerId = @customerId", new {customerId = 123});

                await new RowbotAsyncExecutorBuilder()
                    .FromDataReader(dataReader)
                    .ToExcel(filepath: "c:\\temp\\output.xlsx", sheetName: "MySheet", writeHeaders: true)
                    .ExecuteAsync();
            }

            // Database => CSV (Space complexity: O(1))
            // Sync version
            // using Dapper;
            using (var conn = new SqlConnection(myConnectionString))
            {
                conn.Open();
                var dataReader = conn.ExecuteReader("SELECT * FROM Orders WHERE CustomerId = @customerId", new {customerId = 123});

                new RowbotExecutorBuilder()
                    .FromDataReader(dataReader)
                    .ToCsvUsingCsvHelper(filepath: "c:\\temp\\output.csv", config: new CsvConfiguration(CultureInfo.InvariantCulture), writeHeaders: true)
                    .Execute();
            }

            // Async version
            // using Dapper;
            using (var conn = new SqlConnection(myConnectionString))
            {
                await conn.OpenAsync();
                var dataReader = conn.ExecuteReader("SELECT * FROM Orders WHERE CustomerId = @customerId", new {customerId = 123});

                await new RowbotAsyncExecutorBuilder()
                    .FromDataReader(dataReader)
                    .ToCsvUsingCsvHelper(filepath: "c:\\temp\\output.csv", config: new CsvConfiguration(CultureInfo.InvariantCulture), writeHeaders: true)
                    .ExecuteAsync();
            }

            // Csv => Excel
            // Sync version
            new RowbotExecutorBuilder()
                .FromCsvByCsvHelper(inputStream: File.Open("path//to//file.csv", FileMode.Open), csvConfiguration: new CsvConfiguration(CultureInfo.InvariantCulture), readFirstLineAsHeaders: true)
                .ToExcel(filepath: "c:\\temp\\output.xlsx", sheetName: "MySheet", writeHeaders: true)
                .Execute();

            // Async version
            await new RowbotAsyncExecutorBuilder()
                .FromCsvByCsvHelper(inputStream: File.Open("path//to//file.csv", FileMode.Open), csvConfiguration: new CsvConfiguration(CultureInfo.InvariantCulture), readFirstLineAsHeaders: true)
                .ToExcel(filepath: "c:\\temp\\output.xlsx", sheetName: "MySheet", writeHeaders: true)
                .ExecuteAsync();
        }

        public class Customer
        {
            public long Id { get; set; }
            public string? Name { get; set; }
        }
    }
}