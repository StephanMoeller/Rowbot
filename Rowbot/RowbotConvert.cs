using Rowbot.Core.Targets;
using Rowbot.Execution;
using Rowbot.Targets;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

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
            return GetExecutor(new CsvTarget(outputStream: outputStream, csvConfig: config, writeHeaders: writeHeaders, leaveOpen: true));
        }

        public RowbotExecutor ToExcel(Stream outputStream, string sheetName, bool writeHeaders)
        {
            return GetExecutor(new ExcelTarget(outputStream: outputStream, sheetName: sheetName, writeHeaders: writeHeaders, leaveOpen: true));
        }

        public RowbotExecutor ToDataTable()
        {
            return GetExecutor(new DataTableTarget());
        }

        public RowbotExecutor ToDataReader()
        {
            throw new NotImplementedException();
        }

        public RowbotExecutor ToDynamic()
        {
            return GetExecutor(new DynamicObjectTarget());
        }

        public RowbotExecutor ToObjects<TObjectType>() where TObjectType : new()
        {
            return GetExecutor(new PropertyReflectionTarget<TObjectType>());
        }

        public RowbotExecutor ToCustomTarget(IRowTarget customRowTarget)
        {
            return GetExecutor(customRowTarget);
        }

        private RowbotExecutor GetExecutor(IRowTarget target)
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            return new RowbotExecutor(source: _rowSource, target: target);
        }
    }
}
