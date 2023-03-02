using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ClosedXML.Excel;
using Rowbot.Core.Execution;
using System.Text;
using ClosedXML.Excel;

namespace Benchmarks.Excel
{
    public class Program
    {
        [Benchmark]
        public void Rowbot_AnonymousToExcel_1k_Rows() => Rowbot_AnonymousToExcel_Internal(rowCount: 1_000);
        [Benchmark]
        public void ClosedXml_AnonymousToExcel_1k_Rows() => ClosedXml_AnonymouseToExcel_Internal(rowCount: 1_000);

        [Benchmark]
        public void Rowbot_AnonymousToExcel_100k_Rows() => Rowbot_AnonymousToExcel_Internal(rowCount: 100_000);
        [Benchmark]
        public void ClosedXml_AnonymousToExcel_100k_Rows() => ClosedXml_AnonymouseToExcel_Internal(rowCount: 100_000);

        [Benchmark]
        public void Rowbot_AnonymousToExcel_1m_Rows() => Rowbot_AnonymousToExcel_Internal(rowCount: 1_000_000);
        [Benchmark]
        public void ClosedXml_AnonymousToExcel_1m_Rows() => ClosedXml_AnonymouseToExcel_Internal(rowCount: 1_000_000);

        public void Rowbot_AnonymousToExcel_Internal(int rowCount)
        {
            // Features: Can write to asp.net output stream (both framework and dotnet core)
            // Space allocation: O(1)
            var objects = Enumerable.Range(0, rowCount).Select(num => new MyObject());

            new RowbotExecutorBuilder()
                .FromObjects(objects)
                .ToExcel(filepath: "c:\\temp\\rowbot.xlsx", sheetName: "Sheet1", writeHeaders: true)
                .Execute();
        }

        public void ClosedXml_AnonymouseToExcel_Internal(int rowCount)
        {
            var objects = Enumerable.Range(0, rowCount).Select(num => new MyObject());

            using var wbook = new XLWorkbook();
            var ws = wbook.Worksheets.Add("Sheet1").FirstCell().InsertTable(objects, createTable: false);
            wbook.SaveAs("c:\\temp\\closedXml.xlsx");
        }

        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Program>();
            Console.WriteLine("");
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