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