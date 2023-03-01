﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Rowbot.Targets
{
    public class DataTableTarget : IRowTarget
    {
        private DataTable _table = null;
        private bool _initialized = false;
        private bool _completed = false;
        public DataTableTarget()
        {

        }

        public DataTable GetResult()
        {
            if (!_completed)
                throw new InvalidOperationException("Completed not called yet!");

            return _table;
        }

        public void Complete()
        {
            if (!_initialized)
                throw new InvalidOperationException("Init must be called before Complete()");
            if (_completed)
                throw new InvalidOperationException("Complete already called and can only be called once.");
            _completed = true;
        }

        public void Dispose()
        {
            _table?.Dispose();
        }

        public void Init(ColumnInfo[] columns)
        {
            if (_initialized)
                throw new InvalidOperationException("Init has already been called and can only be called once.");
            _initialized = true;
            _table = new DataTable();
            foreach (var columnInfo in columns)
            {
                _table.Columns.Add(new DataColumn(columnName: columnInfo.Name, dataType: columnInfo.ValueType));
            }
        }

        public void WriteRow(object[] values)
        {
            if (!_initialized)
                throw new InvalidOperationException("Init must be called before WriteRows");
            if (_completed)
                throw new InvalidOperationException("Complete already called. Not allowed to write more rows");
            var row = _table.NewRow();
            for (var i = 0; i < values.Length; i++)
            {
                row[i] = values[i] ?? DBNull.Value;
            }
            _table.Rows.Add(row);
        }
    }
}
