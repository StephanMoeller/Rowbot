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
        static void Main(string[] args)
        {
            var myObjects = Enumerable.Range(0, 10).Select(num => new { });
            DataTable myDataTable = new DataTable();
            string myConnectionString = "...";

            // Objects => Excel
            new RowbotExecutorBuilder()
                .FromObjects(myObjects)
                .ToExcel(filepath: "c:\\temp\\result.xlsx", sheetName: "Sheet1", writeHeaders: true)
                .Execute();

            // Objects => Csv (Space complexity: O(1))
            new RowbotExecutorBuilder()
                .FromObjects(myObjects)
                .ToCsvUsingCsvHelper(filepath: "c:\\temp\\output.csv", config: new CsvConfiguration(CultureInfo.InvariantCulture), writeHeaders: true)
                .Execute();

            // DataTable => Excel (Space complexity: O(1))
            new RowbotExecutorBuilder()
                .FromObjects(myObjects)
                .ToExcel(filepath: "c:\\temp\\output.xlsx", sheetName: "MySheet", writeHeaders: true)
                .Execute();

            // DataTable => Csv (Space complexity: O(1))
            new RowbotExecutorBuilder()
                .FromDataTable(myDataTable)
                .ToCsvUsingCsvHelper(filepath: "c:\\temp\\output.csv", config: new CsvConfiguration(CultureInfo.InvariantCulture), writeHeaders: true)
                .Execute();


            // Database => Excel (Space complexity: O(1))
            // using Dapper;
            using (var conn = new SqlConnection(myConnectionString))
            {
                conn.Open();
                var dataReader = conn.ExecuteReader("SELECT * FROM Orders WHERE CustomerId = @customerId", new { customerId = 123 });

                new RowbotExecutorBuilder()
                    .FromDataReader(dataReader)
                    .ToExcel(filepath: "c:\\temp\\output.xlsx", sheetName: "MySheet", writeHeaders: true)
                    .Execute();
            }

            // Database => CSV (Space complexity: O(1))
            // using Dapper;
            using (var conn = new SqlConnection(myConnectionString))
            {
                conn.Open();
                var dataReader = conn.ExecuteReader("SELECT * FROM Orders WHERE CustomerId = @customerId", new { customerId = 123 });

                new RowbotExecutorBuilder()
                    .FromDataReader(dataReader)
                    .ToCsvUsingCsvHelper(filepath: "c:\\temp\\output.csv", config: new CsvConfiguration(CultureInfo.InvariantCulture), writeHeaders: true)
                    .Execute();
            }

            // DataTable => List of objects (Space complexity: O(1))
            new RowbotExecutorBuilder()
                .FromDataTable(myDataTable)
                .ToObjects<Customer>()
                .Execute(objects =>
                {
                    // NOTE: objects is not allocated in memory but streamed, allowing memory space complexity of O(1)
                    // It is NOT possible to iterate this more than once.
                    foreach (var customer in objects)
                    {
                        // Do something with the customer here
                    }
                });

            // Csv => Excel
            new RowbotExecutorBuilder()
                .FromCsvByCsvHelper(inputStream: File.Open("path//to//file.csv", FileMode.Open), csvConfiguration: new CsvConfiguration(CultureInfo.InvariantCulture), readFirstLineAsHeaders: true)
                .ToExcel(filepath: "c:\\temp\\output.xlsx", sheetName: "MySheet", writeHeaders: true)
                .Execute();
        }

        public class Customer
        {
            public long Id { get; set; }
            public string? Name { get; set; }
        }
    }
}