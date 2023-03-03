
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Rowbot.Execution;
using System.Globalization;
using System.IO;

namespace Benchmarks.Excel
{
    public class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Program>();
            Console.ReadLine();
        }

        [Benchmark]
        public void ObjectsToCsv_Rowbot_1k_rows() => RunCsvTest_Rowbot(1_000);
        [Benchmark]
        public void ObjectsToCsv_CsvHelper_1k_rows() => RunCsvTest_CsvHelper(1_000);

        [Benchmark]
        public void ObjectsToCsv_Rowbot_100k_rows() => RunCsvTest_Rowbot(100_000);
        [Benchmark]
        public void ObjectsToCsv_CsvHelper_100k_rows() => RunCsvTest_CsvHelper(100_000);

        //[Benchmark]
        //public void ObjectsToCsv_Rowbot_1M_rows() => RunCsvTest_Rowbot(1_000_000);
        //[Benchmark]
        //public void ObjectsToCsv_CsvHelper_1M_rows() => RunCsvTest_CsvHelper(1_000_000);


        public static void RunCsvTest_Rowbot(int rowCount)
        {
            var objects = Enumerable.Range(0, rowCount).Select(num => new MyObject());
            using (var ms = new MemoryStream())
            {
                new RowbotExecutorBuilder()
                    .FromObjects(objects)
                    .ToCsv(ms, new Rowbot.Targets.CsvConfig(), writeHeaders: true)
                    .Execute();
            }
                
        }
        public static void RunCsvTest_CsvHelper(int rowCount)
        {
            var objects = Enumerable.Range(0, rowCount).Select(num => new MyObject());

            int flushCounter = 0;
            using (var ms = new MemoryStream())
            {
                var writer = new CsvWriter(new StreamWriter(ms), CultureInfo.InvariantCulture, leaveOpen: false);
                foreach(var obj in objects)
                {
                    writer.WriteRecord(obj);
                    writer.NextRecord();

                    // For a fair comparison, csvHelper is flushed with intervals to ensure no huge memory usage
                    if (flushCounter++ > 100)
                    {
                        writer.Flush();
                        flushCounter = 0;
                    }
                }
                writer.Dispose();
            }
        }


        public class MyObject
        {
            private static string _stringToBeReused = new string('a', 20);
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
}