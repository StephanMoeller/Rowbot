using Rowbot.Core.Targets;
using Rowbot.Execution;
using Rowbot.Targets;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml.Linq;

namespace Rowbot
{
    public static class RowbotConvert
    {
        public static RowbotExecutorBuilder FromDataTable(DataTable dataTable) { throw new NotImplementedException(); }
        public static RowbotExecutorBuilder FromDataReader(IDataReader dataReader) { throw new NotImplementedException(); }
        public static RowbotExecutorBuilder FromObjects<TObjectType>(IEnumerable<TObjectType> objects) { throw new NotImplementedException(); }
        public static RowbotExecutorBuilder FromCustomSource(IRowSource customRowSource) { throw new NotImplementedException(); }
    }

    public class RowbotExecutorBuilder
    {
        private readonly IRowSource _rowSource;

        public RowbotExecutorBuilder(IRowSource rowSource)
        {
            _rowSource = rowSource ?? throw new ArgumentNullException(nameof(rowSource));
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

        public RowbotExecutor<dynamic> ToDynamic()
        {
            return new RowbotExecutor(_rowSource, new DynamicObjectTarget());
        }

        public RowbotExecutor<TObjectType> ToObjects<TObjectType>() where TObjectType : new()
        {
            return ToCustomTarget<TObjectType>(new PropertyReflectionTarget<TObjectType>());
        }

        public RowbotExecutor ToCustomTarget(IRowTarget target)
        {
            return new RowbotExecutor(_rowSource, target);
        }

        public RowbotExecutor<TElement> ToCustomTarget<TElement>(IEnumerableRowTarget<TElement> target)
        {
            return new RowbotExecutor<TElement>(source: _rowSource, target: target);
        }
    }
}
