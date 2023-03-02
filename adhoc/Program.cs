using Rowbot;
using Rowbot.Core.Targets;
using BenchmarkDotNet.Attributes;

namespace AdhocConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            foreach(var e in RowbotConvert.FromCustomSource(null).ToCustomTarget<int>(null).CreateEnumerable())
            {
                
            }

            using (var outputStream = File.Create("c:\\temp\\test1.xlsx"))
            // using (var outputStream = new MemoryStream())
            {
                //using (var target = new ClosedXmlExcelTarget(outputStream: outputStream, sheetName: "SheetName", writeHeaders: true, leaveOpen: true))
                using (var target = new ExcelTarget(outputStream: outputStream, sheetName: "MySheet", writeHeaders: false, leaveOpen: true))
                {
                    int colNum = 1;

                    var range = Enumerable.Range(0, colNum);
                    target.Init(range.Select(num => new ColumnInfo(name: "Col" + num, valueType: typeof(decimal))).ToArray());

                    object[] data = range.Select(num => 123.45678m).Cast<object>().ToArray();

                    for (var i = 0; i < colNum; i++)
                    {
                        target.WriteRow(data);
                    }

                    target.Complete();
                }
                outputStream.Close();
            }

            Console.WriteLine("done");
            Console.ReadLine();
        }
    }


    public class CsvBenchmark
    {
        [Benchmark]
        public void V1() => RunInternal(outputStream => new CsvTarget(outputStream: outputStream, new CsvConfig(){ }));

        //[Benchmark]
        //public void V2() => RunInternal(outputStream => new CsvTargetV2(outputStream: outputStream, new CsvConfig() { }));

        private void RunInternal(Func<Stream, IRowTarget> targetProvider)
        {
            //using (var outputStream = File.Create("c:\\temp\\test1.xlsx"))
            using (var outputStream = new MemoryStream())
            {
                // using (var target = new ClosedXmlExcelTarget(outputStream: zipFileMemoryStream, sheetName: "SheetName", writeHeaders: true, leaveOpen: true))
                using (var target = targetProvider(outputStream))
                {
                    var range = Enumerable.Range(0, 100);
                    target.Init(range.Select(num => new ColumnInfo(name: "Col" + num, valueType: typeof(string))).ToArray());

                    object[] data = range.Select(num => num.ToString()).ToArray();

                    for (var i = 0; i < 50_000; i++)
                    {
                        target.WriteRow(data);
                    }

                    target.Complete();
                }
                outputStream.Close();
            }

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
                using (var target = targetProvider(outputStream))
                {
                    var range = Enumerable.Range(0, 100);
                    target.Init(range.Select(num => new ColumnInfo(name: "Col" + num, valueType: typeof(string))).ToArray());

                    object[] data = range.Select(num => "<" + new string('a', 100)).ToArray();

                    for (var i = 0; i < 1_000; i++)
                    {
                        target.WriteRow(data);
                    }

                    target.Complete();
                }
                outputStream.Close();
            }

        }
    }
}