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

        public RowbotExecutorBuilder FromDataTable(DataTable dataTable) => SetSource(new DataReaderSource(dataTable.CreateDataReader()));
        public RowbotExecutorBuilder FromDataReader(IDataReader dataReader) => SetSource(new DataReaderSource(dataReader));
        public RowbotExecutorBuilder FromObjects<TObjectType>(IEnumerable<TObjectType> objects) => SetSource(PropertyReflectionSource.Create(objects));
        public RowbotExecutorBuilder From(IRowSource customRowSource) => SetSource(customRowSource);
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

        public RowbotExecutor ToCsv(Stream outputStream, CsvConfig config, bool writeHeaders)
        {
            return To(new CsvTarget(outputStream: outputStream, csvConfig: config, writeHeaders: writeHeaders, leaveOpen: true));
        }

        public RowbotExecutor ToCsv(string filepath, CsvConfig config, bool writeHeaders)
        {
            var fs = File.Create(filepath);
            return To(new CsvTarget(outputStream: fs, csvConfig: config, writeHeaders: writeHeaders, leaveOpen: false));
        }


        public RowbotExecutor ToExcel(Stream outputStream, string sheetName, bool writeHeaders)
        {
            return To(new ExcelTarget(outputStream: outputStream, sheetName: sheetName, writeHeaders: writeHeaders, leaveOpen: true));
        }

        public RowbotExecutor ToExcel(string filepath, string sheetName, bool writeHeaders)
        {
            var fs = File.Create(filepath);
            return To(new ExcelTarget(outputStream: fs, sheetName: sheetName, writeHeaders: writeHeaders, leaveOpen: false));
        }

        public RowbotExecutor ToExcelV2(string filepath, string sheetName, bool writeHeaders)
        {
            var fs = File.Create(filepath);
            return To(new ExcelTargetV2(outputStream: fs, sheetName: sheetName, writeHeaders: writeHeaders, leaveOpen: false));
        }

        public RowbotExecutor ToDataTable(DataTable tableToFill)
        {
            return To(new DataTableTarget(tableToFill));
        }

        public RowbotExecutor ToDataReader()
        {
            throw new NotImplementedException();
        }

        public RowbotEnumerableExecutor<TObjectType> ToObjects<TObjectType>() where TObjectType : new()
        {
            return ToCustomTarget(new PropertyReflectionTarget<TObjectType>());
        }

        public RowbotExecutor To(IRowTarget target)
        {
            return new RowbotExecutor(_rowSource, target);
        }

        public RowbotEnumerableExecutor<TElement> ToCustomTarget<TElement>(IEnumerableRowTarget<TElement> target)
        {
            return new RowbotEnumerableExecutor<TElement>(source: _rowSource, target: target);
        }
    }
}
