using CsvHelper.Configuration;
using Rowbot.CsvHelper;
using Rowbot.Sources;
using Rowbot.Targets;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace Rowbot.Execution
{
    public class RowbotAsyncExecutorBuilder
    {
        private IAsyncRowSource _rowSource;

        public RowbotAsyncExecutorBuilder()
        {
        }

        public RowbotAsyncExecutorBuilder FromDataTable(DataTable dataTable)
        {
            return SetSource(new AsyncDataReaderSource(dataTable.CreateDataReader()));
        }

        public RowbotAsyncExecutorBuilder FromDataReader(IDataReader dataReader)
        {
            return SetSource(new AsyncDataReaderSource(dataReader));
        }

        public RowbotAsyncExecutorBuilder FromObjects<TObjectType>(IAsyncEnumerable<TObjectType> objects)
        {
            return SetSource(AsyncPropertyReflectionSource<TObjectType>.Create(objects));
        }

        public RowbotAsyncExecutorBuilder FromDynamic(IAsyncEnumerable<dynamic> objects)
        {
            return SetSource(new AsyncDynamicObjectSource(objects));
        }

        public RowbotAsyncExecutorBuilder FromCsvByCsvHelper(Stream inputStream, CsvConfiguration csvConfiguration, bool readFirstLineAsHeaders)
        {
            return SetSource(new AsyncCsvHelperSource(stream: inputStream, configuration: csvConfiguration, readFirstLineAsHeaders: readFirstLineAsHeaders));
        }

        public RowbotAsyncExecutorBuilder FromCsvByCsvHelper(string filepath, CsvConfiguration csvConfiguration, bool readFirstLineAsHeaders)
        {
            var fs = File.Create(filepath);
            return SetSource(new AsyncCsvHelperSource(stream: fs, configuration: csvConfiguration, readFirstLineAsHeaders: readFirstLineAsHeaders));
        }

        public RowbotAsyncExecutorBuilder From(IAsyncRowSource customRowSource)
        {
            return SetSource(customRowSource);
        }

        private RowbotAsyncExecutorBuilder SetSource(IAsyncRowSource rowSource)
        {
            if (rowSource is null)
            {
                throw new ArgumentNullException(nameof(rowSource));
            }

            if (_rowSource != null)
                throw new ArgumentException("Source already defined in this builder");

            _rowSource = rowSource;
            return this;
        }

        public RowbotAsyncExecutor ToCsvUsingCsvHelper(Stream outputStream, CsvConfiguration config, bool writeHeaders, bool leaveOpen = false)
        {
            return To(new AsyncCsvHelperTarget(stream: outputStream, configuration: config, writeHeaders: writeHeaders, leaveOpen: leaveOpen));
        }

        public RowbotAsyncExecutor ToCsvUsingCsvHelper(string filepath, CsvConfiguration config, bool writeHeaders)
        {
            var fs = File.Create(filepath);
            return To(new AsyncCsvHelperTarget(stream: fs, configuration: config, writeHeaders: writeHeaders, leaveOpen: false));
        }

        public RowbotAsyncExecutor ToExcel(Stream outputStream, string sheetName, bool writeHeaders, bool leaveOpen = false)
        {
            return To(new AsyncExcelTarget(outputStream: outputStream, sheetName: sheetName, writeHeaders: writeHeaders, leaveOpen: leaveOpen));
        }

        public RowbotAsyncExecutor ToExcel(string filepath, string sheetName, bool writeHeaders)
        {
            var fs = File.Create(filepath);
            return To(new AsyncExcelTarget(outputStream: fs, sheetName: sheetName, writeHeaders: writeHeaders, leaveOpen: false));
        }

        public RowbotAsyncExecutor ToDataTable(DataTable tableToFill)
        {
            return To(new AsyncDataTableTarget(tableToFill));
        }

        public RowbotAsyncExecutor ToDataReader()
        {
            throw new NotImplementedException();
        }

        private RowbotAsyncExecutor To(IAsyncRowTarget target)
        {
            return new RowbotAsyncExecutor(_rowSource, target);
        }
    }
}