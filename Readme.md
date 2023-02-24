[TOC]

# CSV use cases

## From DataTable to CSV

## From DataReader to CSV

## From class typed objects to CSV

## From anonymous typed objects to CSV

## From dynamic objects to CSV

## From custom source to CSV

## From CSV to DataTable

## From CSV to DataReader

## From CSV to class typed objects

## From CSV to anonymous typed objects

## From CSV to dynamic objects

## From CSV to custom target

# Excel (.xlsx) use cases

## From DataTable to Excel (.xlsx)

## From DataReader to Excel (.xlsx)

## From class typed objects to Excel (.xlsx)

## From anonymous typed objects to Excel (.xlsx)

## From dynamic objects to Excel (.xlsx)

## From custom source to Excel (.xlsx)

## From Excel (.xlsx) to DataTable

## From Excel (.xlsx) to DataReader

## From Excel (.xlsx) to class typed objects

## From Excel (.xlsx) to anonymous typed objects

## From Excel (.xlsx) to dynamic objects

## From Excel (.xlsx) to custom target

# Other use cases

## From CSV to Excel (.xlsx)

## From Excel (.xlsx) to CSV



# Design decisions

- Seperating source, execution and target
    - Easy plug and play any source with any target
    - Exponential growth of integration possibilites when adding sources. (e.g. When creating a new source, one automatically gets N new integrations where N is the number of available targets. And vice versa, when creating a new target, one automatically gets M new integrations where M is the number of available sources.)
    - Possible to benchmark a seperate source or target or the execution itself.
    - Less code to grasp at once, making it easier to understand, improve and test. E.g. you can look at one element, say a DataReaderSource, in isolation and all improvements to this will be beneficial to everyone using this source no matter what executor and/or target they combine it with.
- "Single row read/write"-design instead of bulk with array
    - Simpler design of the RowSource.ReadRow() and Target.WriteRow() methods and simpler logic in execution methods.
    - Still possible to work with bulks, isolated to a single source or target. A source can still choose to prefetch X elements and feed them one by one. The targets can also work in bulk if they choose, by accepting up to X elements before processing in a bulk.
- Inheritance instead of interface
    - When creating a new source or a new target, one must inherit from RowSource og RowTarget respectively. This has been chosen to enable having guards one place alone. (validation of inputs to sources/targets, validation of outputs from sources/target and internal exceptions when methods are called in unexpected order). If not using inheritance, one should either add all this boilerplate logic to each source/target or to each execution method. Even if wrapping an interface in a guard class inside the existing executors, creating new executors would have to remember to do this. Inheritance ensures write-once logic for this kind of trivial code.
    - Not necessary to test boiler plate guard code in boiler plate tests as it can be tested once, byt creating a class that inherit from RowSource and RowTarget and test it once and for all.
 - Why a custom RowSource and not using IDataReader as the unified source-format?
    - Implementing a custom IDataReader requires one to implement 27 methods, 4 Properties and 2 Indexers. Implementing a custom RowSource requires 2 methods: InitAndGetColumns() and ReadRow(...) and optionally finalizing and cleanup logic in Complete() and Dispose()
- Why a custom RowSource and not using DataTable as the unified source-format?
    - A DataTable requires everything to be hold in memory at once. Any combnation of a non-allocating Rowbot source with a non-garbage-allocating Rowbot target will result in a total non-garbage-allocating solution.

- Todos
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