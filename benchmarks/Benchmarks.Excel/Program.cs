using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ClosedXML.Excel;
using Rowbot.Execution;

namespace Benchmarks.Excel
{
    public class Program
    {
        //[Benchmark]
        //public void Rowbot_ObjectsToExcel_1k_Rows() => Rowbot_ObjectsToExcel_Internal(rowCount: 1_000);
        //[Benchmark]
        //public void MiniExcel_ObjectsToExcel_1k_Rows() => MiniExcel_ObjectseToExcel_Internal(rowCount: 1_000);
        //[Benchmark]
        //public void ClosedXml_ObjectsToExcel_1k_Rows() => ClosedXml_ObjectseToExcel_Internal(rowCount: 1_000);

        public const int runCount = 100_000;

        //[Benchmark]
        //public void Rowbot_ObjectsToExcel_100k_Rows_LongStrings() => Rowbot_ObjectsToExcel_Internal<MyObjectLongStrings>(rowCount: runCount);
        //[Benchmark]
        //public void RowbotV2_ObjectsToExcel_100k_Rows_LongStrings() => Rowbot_ObjectsToExcel_Internal_V2<MyObjectLongStrings>(rowCount: runCount);
        //[Benchmark]
        //public void Rowbot_ObjectsToExcel_100k_Rows_LongStrings_Again() => Rowbot_ObjectsToExcel_Internal<MyObjectLongStrings>(rowCount: runCount);

        //[Benchmark]
        //public void MiniExcel_ObjectsToExcel_100k_Rows_LongStrings() => MiniExcel_ObjectseToExcel_Internal<MyObjectLongStrings>(rowCount: 100_000);

        //[Benchmark]
        //public void Rowbot_ObjectsToExcel_100k_Rows_ShortStrings() => Rowbot_ObjectsToExcel_Internal<MyObjectShortStrings>(rowCount: runCount);
        //[Benchmark]
        //public void RowbotV2_ObjectsToExcel_100k_Rows_ShortStrings() => Rowbot_ObjectsToExcel_Internal_V2<MyObjectShortStrings>(rowCount: runCount);
        //[Benchmark]
        //public void Rowbot_ObjectsToExcel_100k_Rows_ShortStrings_Again() => Rowbot_ObjectsToExcel_Internal<MyObjectShortStrings>(rowCount: runCount);
        //[Benchmark]
        //public void MiniExcel_ObjectsToExcel_100k_Rows_ShortStrings() => MiniExcel_ObjectseToExcel_Internal<MyObjectShortStrings>(rowCount: 100_000);

        [Benchmark]
        public void Rowbot_ObjectsToExcel_100k_Rows_Ints() => Rowbot_ObjectsToExcel_Internal<MyObjectInts>(rowCount: runCount);
        [Benchmark]
        public void RowbotV2_ObjectsToExcel_100k_Rows_Ints() => Rowbot_ObjectsToExcel_Internal_V2<MyObjectInts>(rowCount: runCount);
        [Benchmark]
        public void RowbotV2_ObjectsToExcel_100k_Rows_Ints_Again() => Rowbot_ObjectsToExcel_Internal<MyObjectInts>(rowCount: runCount);
        //[Benchmark]
        //public void MiniExcel_ObjectsToExcel_100k_Rows_Ints() => MiniExcel_ObjectseToExcel_Internal<MyObjectInts>(rowCount: 100_000);
        //[Benchmark]
        //public void ClosedXml_ObjectsToExcel_100k_Rows() => ClosedXml_ObjectseToExcel_Internal(rowCount: 100_000);

        //[Benchmark]
        //public void Rowbot_ObjectsToExcel_1M_Rows() => Rowbot_ObjectsToExcel_Internal(rowCount: 1_000_000);
        //[Benchmark]
        //public void MiniExcel_ObjectsToExcel_1M_Rows() => MiniExcel_ObjectseToExcel_Internal(rowCount: 1_000_000);
        //[Benchmark]
        //public void ClosedXml_ObjectsToExcel_1M_Rows() => ClosedXml_ObjectseToExcel_Internal(rowCount: 1_000_000);

        public void Rowbot_ObjectsToExcel_Internal<T>(int rowCount) where T : new()
        {
            // Features: Can write to asp.net output stream (both framework and dotnet core)
            // Space allocation: O(1)
            var objects = Enumerable.Range(0, rowCount).Select(num => new T());

            new RowbotExecutorBuilder()
                .FromObjects(objects)
                .ToExcel(filepath: "c:\\temp\\rowbot.xlsx", sheetName: "Sheet1", writeHeaders: true)
                .Execute();
        }

        public void Rowbot_ObjectsToExcel_Internal_V2<T>(int rowCount) where T : new()
        {
            // Features: Can write to asp.net output stream (both framework and dotnet core)
            // Space allocation: O(1)
            var objects = Enumerable.Range(0, rowCount).Select(num => new T());

            new RowbotExecutorBuilder()
                .FromObjects(objects)
                .ToExcelV2(filepath: "c:\\temp\\rowbot.xlsx", sheetName: "Sheet1", writeHeaders: true)
                .Execute();
        }

        public void ClosedXml_ObjectseToExcel_Internal(int rowCount)
        {
            var objects = Enumerable.Range(0, rowCount).Select(num => new MyObjectLongStrings());

            using var wbook = new XLWorkbook();
            var ws = wbook.Worksheets.Add("Sheet1").FirstCell().InsertTable(objects, createTable: false);
            wbook.SaveAs("c:\\temp\\closedXml.xlsx");
        }

        public void MiniExcel_ObjectseToExcel_Internal<T>(int rowCount) where T : new()
        {
            var objects = Enumerable.Range(0, rowCount).Select(num => new T());
            MiniExcelLibs.MiniExcel.SaveAs(path: "c:\\temp\\miniexcel.xlsx", value: objects, printHeader: true, overwriteFile: true);
        }

        static void Main(string[] args)
        {
            new Program().Rowbot_ObjectsToExcel_Internal_V2<MyObjectInts>(rowCount: runCount);
                var summary = BenchmarkRunner.Run<Program>();
            Console.ReadLine();
        }
    }

    public class MyObjectLongStrings
    {
        private static string _stringToBeReused = new string('a', 500);
        public string MyString1 { get; } = _stringToBeReused;
        public string MyString2 { get; } = _stringToBeReused;
        public string MyString3 { get; } = _stringToBeReused;
        public string MyString4 { get; } = _stringToBeReused;
        public string MyString5 { get; } = _stringToBeReused;
    }

    public class MyObjectShortStrings
    {
        private static string _stringToBeReused = new string('a', 10);
        public string MyString1 { get; } = _stringToBeReused;
        public string MyString2 { get; } = _stringToBeReused;
        public string MyString3 { get; } = _stringToBeReused;
        public string MyString4 { get; } = _stringToBeReused;
        public string MyString5 { get; } = _stringToBeReused;
    }

    public class MyObjectInts
    {
        public int MyInt1 { get; } = int.MaxValue;
        public int MyInt2 { get; } = int.MinValue;
        public int MyInt3 { get; } = -1;
        public int MyInt4 { get; } = 0;
        public int MyInt5 { get; } = 1;
    }
}