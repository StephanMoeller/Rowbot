


# What is Rowbot?
A fast non-garbage-allocating helper, that will take any source combined with any target nad execute a transfer with a space complexity of O(1) meaning the use of memory will not rise by working with bigger amounts of data.

# Why use Rowbot?
- Fastest dotnet Excel writer on the market
- Modular composition - combine any existing source with any existing target - or even create you own source/target

# Installation
```
dotnet add package Rowbot
```

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

![Excel write benchmark](benchmarks/excel_benchmark_result.png "Benchmark result")

[Benchmark source code](https://github.com/StephanMoeller/Rowbot/blob/main/benchmarks/Benchmarks.Excel/Program.cs)

### Feature comparison

So, Rowbot is the fastest on the market for this exact task. But speed and features are often tradeoffs.

|                                             |       RowBot |  MiniExcel |  ClosedXml |
|--------------------------------------------:|-------------:|-----------:|-----------:|
|                                Memory usage |         O(1) |       O(1) |       O(n) |
|  Can write directly to asp.net OutputStream |          Yes |   	   No | 		No |
|  Supports cell stylings   		          |   	      No | 		    ? |        Yes |
|  Supports referencing   		              |   	      No | 		    ? |        Yes |
|  Supports multi tables   		              |   	      No | 		    ? |        Yes |
|  Supports computed cells		              |   	      No | 		    ? |        Yes |

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
        // It is NOT possible to iterate this more than once.
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
