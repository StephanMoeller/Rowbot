using CsvHelper.Configuration;
using Rowbot.CsvHelper;
using Rowbot.Sources;
using Rowbot.Targets;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml.Linq;

namespace Rowbot.Execution
{
    public class RowbotExecutorBuilder
    {
        private IRowSource _rowSource;

        public RowbotExecutorBuilder()
        {
        }

        public RowbotExecutorBuilder FromDataTable(DataTable dataTable)
        {
            return SetSource(new DataReaderSource(dataTable.CreateDataReader()));
        }

        public RowbotExecutorBuilder FromDataReader(IDataReader dataReader)
        {
            return SetSource(new DataReaderSource(dataReader));
        }

        public RowbotExecutorBuilder FromObjects<TObjectType>(IEnumerable<TObjectType> objects)
        {
            return SetSource(PropertyReflectionSource.Create(objects));
        }

        public RowbotExecutorBuilder FromDynamic(IEnumerable<dynamic> objects)
        {
            return SetSource(new DynamicObjectSource(objects));
        }

        public RowbotExecutorBuilder FromCsvByCsvHelper(Stream inputStream, CsvConfiguration csvConfiguration, bool readFirstLineAsHeaders)
        {
            return SetSource(new CsvHelperSource(stream: inputStream, configuration: csvConfiguration, readFirstLineAsHeaders: readFirstLineAsHeaders));
        }

        public RowbotExecutorBuilder FromCsvByCsvHelper(string filepath, CsvConfiguration csvConfiguration, bool readFirstLineAsHeaders)
        {
            var fs = File.Create(filepath);
            return SetSource(new CsvHelperSource(stream: fs, configuration: csvConfiguration, readFirstLineAsHeaders: readFirstLineAsHeaders));
        }

        public RowbotExecutorBuilder From(IRowSource customRowSource)
        {
            return SetSource(customRowSource);
        }

        private RowbotExecutorBuilder SetSource(IRowSource rowSource)
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

        public RowbotExecutor ToCsvUsingCsvHelper(Stream outputStream, CsvConfiguration config, bool writeHeaders, bool leaveOpen = false)
        {
            return new RowbotExecutor(_rowSource, new CsvHelperTarget(stream: outputStream, configuration: config, writeHeaders: writeHeaders, leaveOpen: leaveOpen));
        }

        public RowbotExecutor ToCsvUsingCsvHelper(string filepath, CsvConfiguration config, bool writeHeaders)
        {
            var fs = File.Create(filepath);
            return new RowbotExecutor(_rowSource, new CsvHelperTarget(stream: fs, configuration: config, writeHeaders: writeHeaders, leaveOpen: false));
        }

        public RowbotExecutor ToExcel(Stream outputStream, string sheetName, bool writeHeaders, bool leaveOpen = false)
        {
            return new RowbotExecutor(_rowSource, new ExcelTarget(outputStream: outputStream, sheetName: sheetName, writeHeaders: writeHeaders, leaveOpen: leaveOpen));
        }

        public RowbotExecutor ToExcel(string filepath, string sheetName, bool writeHeaders)
        {
            var fs = File.Create(filepath);
            return new RowbotExecutor(_rowSource, new ExcelTarget(outputStream: fs, sheetName: sheetName, writeHeaders: writeHeaders, leaveOpen: false));
        }

        public RowbotExecutor ToDataTable(DataTable tableToFill)
        {
            return new RowbotExecutor(_rowSource, new DataTableTarget(tableToFill));
        }

        public RowbotExecutor ToDataReader()
        {
            throw new NotImplementedException();
        }

        public RowbotEnumerableExecutor<TObjectType> ToObjects<TObjectType>(Action<IEnumerable<TObjectType>> consumer) where TObjectType : new()
        {
            return new RowbotEnumerableExecutor<TObjectType>(
                source: _rowSource,
                target: new PropertyReflectionTarget<TObjectType>(),
                consumer: consumer
            );
        }
    }
}