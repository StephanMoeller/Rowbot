Design decisions

- Seperating source, execution and target
    - Easy plug and play any source with any target
    - Exponential growth of integration possibilites when adding sources. (e.g. When creating a new source, one automatically gets N new integrations where N is the number of available targets. And vice versa, when creating a new target, one automatically gets M new integrations where M is the number of available sources.)
    - Possible to benchmark a seperate source or target or the execution itself for both cpu and memory usage.
    - Less code to grasp at once, making it easier to understand and improve. E.g. you can look at the executor alone and only focus on the multithreaded aspect of execution without mixing the concept with loading or saving data at any point.
    - Easier to test seperate parts
- Single entry design over bulk with array
    - Simpler design of the RowSource.ReadRow() and Target.WriteRow() methods and simpler logic in execution methods.
    - Still possible to work with bulks, isolated to a single source or target. A source can still choose to prefetch X elements and feed them one by one. The targets can also work in bulk if they choose, by accepting up to X elements before processing in a bulk.
- Inheritance instead of interface
    - When creating a new source or a new target, one must inherit from RowSource og RowTarget respectively. This has been chosen to be able to have guards one place alone. (validation of inputs to sources/targets, validation of outputs from sources/target and internal execptions when methods are called in unexpected order). If not using inherticate, one should either add all this boilerplate logic to each source/target or to each execution method. Even if wrapping an interface in a guard class inside the existing executors, creating new executors would have to remember to do this. Inheritance ensures write-once logic for this kind of trivial code.
    - Not necessary to test boiler plate guard code in boiler plate tests as it can be tested once, byt creating a class that inherit from RowSource and RowTarget and test it once and for all.
 - Why a custom RowSource and not using IDataReader?
    - Implementing a custom IDataReader requires one to implement 27 methods, 4 Properties and 2 Indexers. Implementing a custom RowSource requires 2 methods: InitAndGetColumns() and ReadRow(...) and optionally finalizing and cleanup logic in Complete() and Dispose()

Todos
    - Grow buffer inside CsvTarget (If not replacing with ExcelHelper)
    - Decide whether to create seperate projects for Excel support (MiniExcel) and CSV support (ExcelHelper)
    - Create tests for execution methods
    - CancellationToken in ReadRow and WriteRow?
    - Add a DynamicTarget (just because it would be an easy thing to do)
    - Add a test with Sqlite loading data and processing it to excel as a real life usage case test
    - Create use cases and benchmarks for the readme
    - Stats for execution (Wait times per source + entry counts processed)
