using Rowbot.CsvHelper;
using Rowbot.Execution;
using Rowbot.Sources;
using Rowbot.Targets;
using Rowbot;
using System.Globalization;

namespace AdhocConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" };
            using (var fs = File.Create("c:\\temp\\output.csv"))
            {
                using (var target = new CsvHelperTarget(stream: fs, configuration: config, writeHeaders: true))
                {
                    target.Init(new Rowbot.ColumnInfo[]{
                        new ColumnInfo(name: "Col1", typeof(string)),
                        new ColumnInfo(name: "Col2", typeof(string)),
                        new ColumnInfo(name: "Col3", typeof(string))
                    });
                    var values = new object[] { new string('a', 300), new string('b', 300), new string('c', 300) };
                    for(var i = 0; i < 1_000_000; i++)
                    {
                        target.WriteRow(values);
                    }
                }
            }

        }
    }
}