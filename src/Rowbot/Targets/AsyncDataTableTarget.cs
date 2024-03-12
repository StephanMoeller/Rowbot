using System;
using System.Data;
using System.Threading.Tasks;

namespace Rowbot.Targets
{
    public class AsyncDataTableTarget : IAsyncRowTarget
    {
        private readonly DataTable _table = null;
        
        public AsyncDataTableTarget(DataTable tableToFill)
        {
            if (tableToFill.Columns.Count > 0 || tableToFill.Rows.Count > 0)
                throw new ArgumentException("Provided table must be empty. Columns and/or rows found.");
            _table = tableToFill;
        }

        public Task CompleteAsync()
        {
            return Task.CompletedTask;
        }

        public Task InitAsync(ColumnInfo[] columns)
        {
            foreach (var columnInfo in columns)
            {
                _table.Columns.Add(new DataColumn(columnName: columnInfo.Name, dataType: columnInfo.ValueType));
            }

            return Task.CompletedTask;
        }

        public Task WriteRowAsync(object[] values)
        {
            var row = _table.NewRow();
            for (var i = 0; i < values.Length; i++)
            {
                row[i] = values[i] ?? DBNull.Value;
            }
            _table.Rows.Add(row);
            
            return Task.CompletedTask;
        }
    }
}
