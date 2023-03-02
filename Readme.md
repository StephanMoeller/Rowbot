
# What is Rowbot?
A fast non-garbage-allocating helper, that will take any source combined with any target nad execute a transfer without a space complexity of O(1).

# Benchmarks
Objects => Excel

``HEY``
|                               Method |         Mean |      Error |     StdDev |
|------------------------------------- |-------------:|-----------:|-----------:|
|      Rowbot_AnonymousToExcel_1k_Rows |     12.89 ms |   0.079 ms |   0.066 ms |
|   MiniExcel_AnonymousToExcel_1k_Rows |     27.57 ms |   0.182 ms |   0.161 ms |
|   ClosedXml_AnonymousToExcel_1k_Rows |     50.04 ms |   0.915 ms |   0.811 ms |
|    Rowbot_AnonymousToExcel_100k_Rows |  1,155.30 ms |   3.484 ms |   3.259 ms |
| MiniExcel_AnonymousToExcel_100k_Rows |  2,449.91 ms |  43.806 ms |  40.976 ms |
| ClosedXml_AnonymousToExcel_100k_Rows |  5,258.25 ms |  33.401 ms |  29.609 ms |
|      Rowbot_AnonymousToExcel_1M_Rows | 11,417.82 ms |  22.955 ms |  20.349 ms |
|   MiniExcel_AnonymousToExcel_1M_Rows | 23,939.30 ms | 109.274 ms | 102.215 ms |
|   ClosedXml_AnonymousToExcel_1M_Rows | 53,443.07 ms |  73.527 ms |  61.399 ms |

# Design decisions

- Seperating source, execution and target
    - Easy plug and play any source with any target
    - Exponential growth of integration possibilites when adding sources. (e.g. When creating a new source, one automatically gets N new integrations where N is the number of available targets. And vice versa, when creating a new target, one automatically gets M new integrations where M is the number of available sources.)
    - Possible to benchmark a seperate source or target or the execution itself.
    - Less code to grasp at once, making it easier to understand, improve and test. E.g. you can look at one element, say a DataReaderSource, in isolation and all improvements to this will be beneficial to everyone using this source no matter what executor and/or target they combine it with.
- "Single row read/write"-design instead of bulk with array
    - Simpler design of the RowSource.ReadRow() and Target.WriteRow() methods and simpler logic in execution methods.
    - Still possible to work with bulks, isolated to a single source or target. A source can still choose to prefetch X elements and feed them one by one. The targets can also work in bulk if they choose, by accepting up to X elements before processing in a bulk.

- Todos
    - O(1) space allocation for e.g. Csv => Objects / Dynamic
    - Add csvHelper source and targets
    - Add som excel source and targets
    - Create tests for execution methods
    - CancellationToken in ReadRow and WriteRow?
    - Add a DynamicTarget (just because it would be an easy thing to do)
    - Add a test with Sqlite loading data and processing it to excel as a real life usage case test
    - Create use cases and benchmarks for the readme
    - Stats for execution (Wait times per source + entry counts processed)
    - Allow sheet as input in ClosedXmlExcelTarget to allow composing a multi sheet document using Rowbot
    - Make the different sources and targets easily discovarable
    - Make is visible to the consumer which source and targets are non-allocating and which holds the enties workload in memory during processing.
    - Ranem entire project to RowGun.net
    - CsvTarget should have a writeHeaders flag in it to allow on/off