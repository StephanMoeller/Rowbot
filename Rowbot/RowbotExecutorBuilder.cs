using Rowbot.Core.Targets;
using Rowbot.Execution;
using Rowbot.Sources;
using Rowbot.Targets;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml.Linq;

namespace Rowbot
{
    public class RowbotExecutorBuilder
    {
        private IRowSource _rowSource;
        
        public RowbotExecutorBuilder()
        {
        }

        public RowbotExecutorBuilder FromDataTable(DataTable dataTable) => SetSource(new DataTableSource(dataTable));

        public RowbotExecutorBuilder FromDataReader(IDataReader dataReader) => SetSource(new DataReaderSource(dataReader));
        public RowbotExecutorBuilder FromObjects<TObjectType>(IEnumerable<TObjectType> objects) => SetSource(PropertyReflectionSource.Create(objects));
        public RowbotExecutorBuilder FromCustomSource(IRowSource customRowSource) => SetSource(customRowSource);
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
            return ToCustomTarget(new CsvTarget(outputStream: outputStream, csvConfig: config, writeHeaders: writeHeaders, leaveOpen: true));
        }

        public RowbotExecutor ToExcel(Stream outputStream, string sheetName, bool writeHeaders)
        {
            return ToCustomTarget(new ExcelTarget(outputStream: outputStream, sheetName: sheetName, writeHeaders: writeHeaders, leaveOpen: true));
        }

        public RowbotExecutor ToDataTable()
        {
            return ToCustomTarget(new DataTableTarget());
        }

        public RowbotExecutor ToDataReader()
        {
            throw new NotImplementedException();
        }

        public RowbotEnumerableExecutor<TObjectType> ToObjects<TObjectType>() where TObjectType : new()
        {
            return ToCustomTarget<TObjectType>(new PropertyReflectionTarget<TObjectType>());
        }

        public RowbotExecutor ToCustomTarget(IRowTarget target)
        {
            return new RowbotExecutor(_rowSource, target);
        }

        public RowbotEnumerableExecutor<TElement> ToCustomTarget<TElement>(IEnumerableRowTarget<TElement> target)
        {
            return new RowbotEnumerableExecutor<TElement>(source: _rowSource, target: target);
        }
    }
}
