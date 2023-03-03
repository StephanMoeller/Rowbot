


# What is Rowbot?
A fast non-garbage-allocating helper, that will take any source combined with any target nad execute a transfer without a space complexity of O(1).

# Rowbot
## Examples
### Create Excel file from list of objects
``` csharp
	// Objects => Excel
	new RowbotExecutorBuilder()
       .FromObjects(objects)
       .ToExcel(filepath: "c:\\temp\\rowbot.xlsx", sheetName: "Sheet1", writeHeaders: true)
       .Execute();
```
|                               Method |         Mean |      Error |     StdDev |
|-------------------------------------:|-------------:|-----------:|-----------:|
|      Rowbot_ObjectsToExcel_1k_Rows |     12.89 ms |   0.079 ms |   0.066 ms |
|   MiniExcel_ObjectsToExcel_1k_Rows |     27.57 ms |   0.182 ms |   0.161 ms |
|   ClosedXml_ObjectsToExcel_1k_Rows |     50.04 ms |   0.915 ms |   0.811 ms |
|    Rowbot_ObjectsToExcel_100k_Rows |  1,155.30 ms |   3.484 ms |   3.259 ms |
| MiniExcel_ObjectsToExcel_100k_Rows |  2,449.91 ms |  43.806 ms |  40.976 ms |
| ClosedXml_ObjectsToExcel_100k_Rows |  5,258.25 ms |  33.401 ms |  29.609 ms |
|      Rowbot_ObjectsToExcel_1M_Rows | 11,417.82 ms |  22.955 ms |  20.349 ms |
|   MiniExcel_ObjectsToExcel_1M_Rows | 23,939.30 ms | 109.274 ms | 102.215 ms |
|   ClosedXml_ObjectsToExcel_1M_Rows | 53,443.07 ms |  73.527 ms |  61.399 ms |

[Benchmark source code](https://github.com/StephanMoeller/Rowbot/blob/main/Benchmarks.Excel/Program.cs)

Other features when writing excel from objects:

|                                    |       RowBot |  MiniExcel |  ClosedXml |
|-----------------------------------:|-------------:|-----------:|-----------:|
|                       Memory usage |         O(1) |       O(1) |       O(n) |
|  Can write to asp.net OutputStream |     		Yes |   	  No | 		   No |

### More examples
``` csharp
// Objects => CSV (Space complexity: O(1))
new RowbotExecutorBuilder()
    .FromObjects(myObjects)
    .ToCsv(filepath: "c:\\temp\\rowbot.csv", config: new CsvConfig() { Delimiter = ';', Quote = '\'' }, writeHeaders: true)
    .Execute();
```

``` csharp
// Objects => Excel (Space complexity: O(1))
new RowbotExecutorBuilder()
    .FromObjects(myObjects)
    .ToExcel(filepath: "c:\\temp\\rowbot.xlsx", sheetName: "MySheet", writeHeaders: true)
    .Execute();
```

``` csharp
// DataTable => CSV (Space complexity: O(1))
new RowbotExecutorBuilder()
    .FromDataTable(myDataTable)
    .ToCsv(filepath: "c:\\temp\\rowbot.csv", config: new CsvConfig() { Delimiter = ';', Quote = '\'' }, writeHeaders: true)
    .Execute();
```

``` csharp
// DataTable => Excel (Space complexity: O(1))
new RowbotExecutorBuilder()
    .FromDataTable(myDataTable)
    .ToExcel(filepath: "c:\\temp\\rowbot.xlsx", sheetName: "MySheet", writeHeaders: true)
    .Execute();
```

``` csharp
// Database => CSV (Space complexity: O(1))
using Dapper;
using (var conn = new SqlConnection(myConnectionString))
{
    conn.Open();
    var dataReader = conn.ExecuteReader("SELECT * FROM Orders WHERE CustomerId = @customerId", new { customerId = 123 });

    new RowbotExecutorBuilder()
        .FromDataReader(dataReader)
        .ToCsv(filepath: "c:\\temp\\rowbot.csv", config: new CsvConfig() { Delimiter = ';', Quote = '\'' }, writeHeaders: true)
        .Execute();
}
```

``` csharp
// Database => Excel (Space complexity: O(1))
using Dapper;
using (var conn = new SqlConnection(myConnectionString))
{
    conn.Open();
    var dataReader = conn.ExecuteReader("SELECT * FROM Orders WHERE CustomerId = @customerId", new { customerId = 123 });

    new RowbotExecutorBuilder()
        .FromDataReader(dataReader)
        .ToExcel(filepath: "c:\\temp\\rowbot.xlsx", sheetName: "MySheet", writeHeaders: true)
        .Execute();
}
```

``` csharp
// DataTable => List of objects (Space complexity: O(1))
new RowbotExecutorBuilder()
    .FromDataTable(myDataTable)
    .ToObjects<Customer>()
    .Execute(objects =>
    {
        // NOTE: objects is not allocated in memory but streamed, allowing memory space complexity of O(1)
        foreach (var customer in objects)
        {
            // Do something with the customer here
        }
    });
```

# Notes

- Design decision: Seperating source, execution and target
    - Easy plug and play any source with any target
    - Exponential growth of integration possibilites when adding sources. (e.g. When creating a new source, one automatically gets N new integrations where N is the number of available targets. And vice versa, when creating a new target, one automatically gets M new integrations where M is the number of available sources.)
    - Possible to benchmark a seperate source or target or the execution itself.
    - Less code to grasp at once, making it easier to understand, improve and test. E.g. you can look at one element, say a DataReaderSource, in isolation and all improvements to this will be beneficial to everyone using this source no matter what executor and/or target they combine it with.

- Todos
    - Fix problem with Exceltarget always closing stream
    - Stats for execution (Wait times per source + entry counts processed)
    - CancellationToken in ReadRow and WriteRow?
    - Add a test with Sqlite loading data and processing it to excel as a real life usage case test
    - Exceltarget: Test with reuse of array instead of creating new params-arrays over and over