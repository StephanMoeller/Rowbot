using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Rowbot.Targets
{
    public class DataTableTarget : IRowTarget
    {
        private readonly DataTable _table = null;
        public DataTableTarget(DataTable tableToFill)
        {
            if (tableToFill.Columns.Count > 0 || tableToFill.Rows.Count > 0)
                throw new ArgumentException("Provided table must be empty. Columns and/or rows found.");
            _table = tableToFill;
        }

        public void Complete()
        {
            //
        }

        public void Init(ColumnInfo[] columns)
        {
            foreach (var columnInfo in columns)
            {
                _table.Columns.Add(new DataColumn(columnName: columnInfo.Name, dataType: columnInfo.ValueType));
            }
        }

        public void WriteRow(object[] values)
        {
            var row = _table.NewRow();
            for (var i = 0; i < values.Length; i++)
            {
                row[i] = values[i] ?? DBNull.Value;
            }
            _table.Rows.Add(row);
        }
    }
}
