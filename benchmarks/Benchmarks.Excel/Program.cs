using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ClosedXML.Excel;
using Rowbot.Core.Execution;

namespace Benchmarks.Excel
{
    public class Program
    {
        [Benchmark]
        public void Rowbot_ObjectsToExcel_1k_Rows() => Rowbot_ObjectsToExcel_Internal(rowCount: 1_000);
        [Benchmark]
        public void MiniExcel_ObjectsToExcel_1k_Rows() => MiniExcel_ObjectseToExcel_Internal(rowCount: 1_000);
        [Benchmark]
        public void ClosedXml_ObjectsToExcel_1k_Rows() => ClosedXml_ObjectseToExcel_Internal(rowCount: 1_000);

        [Benchmark]
        public void Rowbot_ObjectsToExcel_100k_Rows() => Rowbot_ObjectsToExcel_Internal(rowCount: 100_000);
        [Benchmark]
        public void MiniExcel_ObjectsToExcel_100k_Rows() => MiniExcel_ObjectseToExcel_Internal(rowCount: 100_000);
        [Benchmark]
        public void ClosedXml_ObjectsToExcel_100k_Rows() => ClosedXml_ObjectseToExcel_Internal(rowCount: 100_000);

        [Benchmark]
        public void Rowbot_ObjectsToExcel_1M_Rows() => Rowbot_ObjectsToExcel_Internal(rowCount: 1_000_000);
        [Benchmark]
        public void MiniExcel_ObjectsToExcel_1M_Rows() => MiniExcel_ObjectseToExcel_Internal(rowCount: 1_000_000);
        [Benchmark]
        public void ClosedXml_ObjectsToExcel_1M_Rows() => ClosedXml_ObjectseToExcel_Internal(rowCount: 1_000_000);

        public void Rowbot_ObjectsToExcel_Internal(int rowCount)
        {
            // Features: Can write to asp.net output stream (both framework and dotnet core)
            // Space allocation: O(1)
            var objects = Enumerable.Range(0, rowCount).Select(num => new MyObject());

            new RowbotExecutorBuilder()
                .FromObjects(objects)
                .ToExcel(filepath: "c:\\temp\\rowbot.xlsx", sheetName: "Sheet1", writeHeaders: true)
                .Execute();
        }

        public void ClosedXml_ObjectseToExcel_Internal(int rowCount)
        {
            var objects = Enumerable.Range(0, rowCount).Select(num => new MyObject());

            using var wbook = new XLWorkbook();
            var ws = wbook.Worksheets.Add("Sheet1").FirstCell().InsertTable(objects, createTable: false);
            wbook.SaveAs("c:\\temp\\closedXml.xlsx");
        }

        public void MiniExcel_ObjectseToExcel_Internal(int rowCount)
        {
            var objects = Enumerable.Range(0, rowCount).Select(num => new MyObject());
            MiniExcelLibs.MiniExcel.SaveAs(path: "c:\\temp\\miniexcel.xlsx", value: objects, printHeader: true, overwriteFile: true);
        }

        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Program>();
            Console.ReadLine();
        }
    }

    public class MyObject
    {
        private static string _stringToBeReused = new string('a', 200);
        public string MyString1 { get; } = _stringToBeReused;
        public string MyString2 { get; } = _stringToBeReused;
        public string MyString3 { get; } = _stringToBeReused;
        public string MyString4 { get; } = _stringToBeReused;
        public string MyString5 { get; } = _stringToBeReused;
        public int MyInt1 { get; } = int.MaxValue;
        public int MyInt2 { get; } = int.MinValue;
        public int MyInt3 { get; } = -1;
        public int MyInt4 { get; } = 0;
        public int MyInt5 { get; } = 1;
    }
}