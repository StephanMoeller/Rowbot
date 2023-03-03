using Rowbot;
using Rowbot.Core.Targets;
using BenchmarkDotNet.Attributes;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using BenchmarkDotNet.Running;

namespace AdhocConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ExcelTargetBenchmark>();
            Console.ReadLine();
            // Testing checkins
            //int colCount = 25;
            //for (var compressionLevel = 0; compressionLevel <= 9; compressionLevel++)
            //{
            //    var sw = Stopwatch.StartNew();
            //    int rowCount = 100_000;

            //    var columns = Enumerable.Range(0, 25).Select(num => new ColumnInfo(name: "Col" + num, valueType: typeof(string))).ToArray();
            //    var data = Enumerable.Range(0, 25).Select(num => new string('a', 100)).Cast<object>().ToArray();

            //    using (var fs = File.Create($"c:\\temp\\outputv2_level_{compressionLevel}.xlsx"))
            //    using (var target = new ExcelTargetV2(fs, sheetName: "My sheet", writeHeaders: true, compressionLevel: compressionLevel, leaveOpen: true))
            //    {
            //        target.Init(columns);
            //        for (var i = 0; i < rowCount; i++)
            //        {
            //            target.WriteRow(data);
            //        }
            //        target.Complete();
            //    }

            //    Console.WriteLine("Took " + sw.Elapsed + " with compressionLevel " + compressionLevel);
            //}


            Console.WriteLine("done");
            Console.ReadLine();
        }
    }


    public class ExcelTargetBenchmark
    {
        [Benchmark]
        public void V1() => RunInternal(outputStream => new ExcelTarget(outputStream: outputStream, sheetName: "sheet1", writeHeaders: true));
        
        public void RunInternal(Func<Stream, IRowTarget> targetProvider)
        {
            //using (var outputStream = File.Create("c:\\temp\\test1.xlsx"))
            using (var outputStream = new MemoryStream())
            {
                // using (var target = new ClosedXmlExcelTarget(outputStream: zipFileMemoryStream, sheetName: "SheetName", writeHeaders: true, leaveOpen: true))
                var target = targetProvider(outputStream);

                var range = Enumerable.Range(0, 100);
                target.Init(range.Select(num => new ColumnInfo(name: "Col" + num, valueType: typeof(string))).ToArray());

                object[] data = range.Select(num => num.ToString()).ToArray();

                for (var i = 0; i < 50_000; i++)
                {
                    target.WriteRow(data);
                }

                target.Complete();

                outputStream.Close();
            }

        }
    }
}