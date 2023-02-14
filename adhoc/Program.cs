using Rowbot.Execution;
using Rowbot.Sources;
using Rowbot.Targets;

namespace AdhocConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var elements = Enumerable.Range(0, 10).Select(e => new
            {
                Name = "Hello there and wel\"come! ",
                Age = (short)29,
                Age2 = (int)9898,
                Age3 = (long)98989898
            });

            using (var fs = File.Create("c:\\temp\\output.csv"))
            using (var source = PropertyReflectionSource.Create(elements))
            using (var target = new CsvTarget(outputStream: fs, new CsvConfig()))
            {
                RowbotExecutor.ExecuteDualThreaded(source, target, maxQueueLength: 2);
            }

            MiniExcel.SaveAs(path: "c:\\temp\\MiniExcel.csv", value: elements, printHeader: true, excelType: ExcelType.CSV);
            Console.WriteLine("Hello, World!");
        }
    }
}