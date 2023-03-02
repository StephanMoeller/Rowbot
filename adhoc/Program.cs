using Rowbot;
using Rowbot.Core.Targets;
using BenchmarkDotNet.Attributes;
using Rowbot.Core.Execution;
using System.IO.Compression;
using System;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Presentation;
using System.Text.Unicode;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace AdhocConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //using (FileStream zipToOpen = new FileStream("c:\\temp\\hello.zip", FileMode.Create))
            //{
            //    using (ZipArchive archive = new ZipArchive(new LoggingStream(zipToOpen, logMsg => Console.WriteLine(logMsg)), ZipArchiveMode.Update))
            //    {
            //        ZipArchiveEntry readmeEntry = archive.CreateEntry("Readme.txt");
            //        using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
            //        {
            //            writer.WriteLine("Information about this package.");
            //            writer.WriteLine("========================");
            //        }
            //    }
            //}







            // 'using' statements guarantee the stream is closed properly which is a big source
            // of problems otherwise.  Its exception safe as well which is great.
            using (ZipOutputStream OutputStream = new ZipOutputStream(new LoggingStream(File.Create("c:\\temp\\hello2.zip"), logMsg => Console.WriteLine(logMsg))))
            {
                // Define the compression level
                // 0 - store only to 9 - means best compression
                OutputStream.SetLevel((int)CompressionLevel.NoCompression);

                byte[] buffer = new byte[4096];

                // Using GetFileName makes the result compatible with XP
                // as the resulting path is not absolute.
                ZipEntry entry = new ZipEntry("Hello.txt");

                // Setup the entry data as required.

                // Crc and size are handled by the library for seakable streams
                // so no need to do them here.

                // Could also use the last write time or similar for the file.
                entry.DateTime = DateTime.Now;
                OutputStream.PutNextEntry(entry);

                var bytes = Encoding.UTF8.GetBytes("Hello!");
                OutputStream.Write(bytes, 0, bytes.Length);

                // Finish/Close arent needed strictly as the using statement does this automatically

                // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                // the created file would be invalid.
                OutputStream.Finish();

                // Close is important to wrap things up and unlock the file.
                OutputStream.Close();

                Console.WriteLine("Files successfully compressed");
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