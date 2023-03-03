using Rowbot;
using Rowbot.Core.Targets;
using BenchmarkDotNet.Attributes;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace AdhocConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int colCount = 25;
            int rowCount = 100;

            var columns = Enumerable.Range(0, 25).Select(num => new ColumnInfo(name: "Col" + num, valueType: typeof(string))).ToArray();
            var data = Enumerable.Range(0, 25).Select(num => new string('a', 100)).Cast<object>().ToArray();

            using (var fs = File.Create("c:\\temp\\outputv1.xlsx"))
            using (var target = new ExcelTargetV2(fs, sheetName: "My sheet", writeHeaders: true, leaveOpen: true))
            {
                target.Init(columns);
                for (var i = 0; i < rowCount; i++)
                {
                    target.WriteRow(data);
                }
                target.Complete();
            }


            Console.WriteLine("done");
            Console.ReadLine();
        }
    }


    public class CsvBenchmark
    {
        [Benchmark]
        public void V1() => RunInternal(outputStream => new CsvTarget(outputStream: outputStream, new CsvConfig() { }));

        //[Benchmark]
        //public void V2() => RunInternal(outputStream => new CsvTargetV2(outputStream: outputStream, new CsvConfig() { }));

        private void RunInternal(Func<Stream, IRowTarget> targetProvider)
        {
            ////using (var outputStream = File.Create("c:\\temp\\test1.xlsx"))
            //using (var outputStream = new MemoryStream())
            //{
            //    // using (var target = new ClosedXmlExcelTarget(outputStream: zipFileMemoryStream, sheetName: "SheetName", writeHeaders: true, leaveOpen: true))
            //    using (var target = targetProvider(outputStream))
            //    {
            //        var range = Enumerable.Range(0, 100);
            //        target.Init(range.Select(num => new ColumnInfo(name: "Col" + num, valueType: typeof(string))).ToArray());

            //        object[] data = range.Select(num => num.ToString()).ToArray();

            //        for (var i = 0; i < 50_000; i++)
            //        {
            //            target.WriteRow(data);
            //        }

            //        target.Complete();
            //    }
            //    outputStream.Close();
            //}

        }
    }

    public class ExcelBenchmark
    {
        [Benchmark]
        public void ExcelTarget() => RunInternal(outputStream => new ExcelTarget(outputStream: outputStream, sheetName: "MySheet", writeHeaders: false, leaveOpen: true));
        //[Benchmark]
        //public void ExcelTargetV2() => RunInternal(outputStream => new ExcelTargetV2(outputStream: outputStream, writeHeaders: false, leaveOpen: true));

        private void RunInternal(Func<Stream, IRowTarget> targetProvider)
        {
            //using (var outputStream = File.Create("c:\\temp\\test1.xlsx"))
            using (var outputStream = new MemoryStream())
            {
                // using (var target = new ClosedXmlExcelTarget(outputStream: zipFileMemoryStream, sheetName: "SheetName", writeHeaders: true, leaveOpen: true))
                //using (var target = targetProvider(outputStream))
                //{
                //    var range = Enumerable.Range(0, 100);
                //    target.Init(range.Select(num => new ColumnInfo(name: "Col" + num, valueType: typeof(string))).ToArray());

                //    object[] data = range.Select(num => "<" + new string('a', 100)).ToArray();

                //    for (var i = 0; i < 1_000; i++)
                //    {
                //        target.WriteRow(data);
                //    }

                //    target.Complete();
                //}
                //outputStream.Close();
            }

        }
    }
}