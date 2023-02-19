﻿using Rowbot.CsvHelper;
using Rowbot.Execution;
using Rowbot.Sources;
using Rowbot.Targets;
using Rowbot;
using System.Globalization;
using Rowbot.Core.Targets;
using System.Text;
using CsvHelper.Configuration;
using System.Diagnostics;

namespace AdhocConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var sw = Stopwatch.StartNew();

            //using (var fs = File.Create("c:\\temp\\output.csv"))
            //{
            //    var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", NewLine = "\r\n" };
            //    using (var target = new CsvHelperTarget(stream: fs, configuration: config, writeHeaders: true))
            //    {
            //        target.Init(new Rowbot.ColumnInfo[]{
            //            new ColumnInfo(name: "Col1", typeof(string)),
            //            new ColumnInfo(name: "Col2", typeof(string)),
            //            new ColumnInfo(name: "Col3", typeof(string))
            //        });
            //        var values = new object[] { new string('a', 30), new string('b', 30), new string('c', 30) };
            //        for (var i = 0; i < 10_000_000; i++)
            //        {
            //            target.WriteRow(values);
            //        }
            //    }
            //}

            using (var fs = File.Create("c:\\temp\\output_turbo.csv"))
            {
                using (var target = new CsvTarget(outputStream: fs, csvConfig: new CsvConfig() { Delimiter = ';', Newline = "\r\n", Quote = '"', Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false) }))
                {
                    target.Init(new Rowbot.ColumnInfo[]{
                        new ColumnInfo(name: "Col1", typeof(string)),
                        new ColumnInfo(name: "Col2", typeof(string)),
                        new ColumnInfo(name: "Col3", typeof(string))
                    });
                    var values = new object[] { new string('a', 30), new string('b', 30), new string('c', 30) };
                    for (var i = 0; i < 10_000_000; i++)
                    {
                        target.WriteRow(values);
                    }
                }
            }
            Console.WriteLine(sw.ElapsedMilliseconds);
        }
    }
}