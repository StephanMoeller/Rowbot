﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rowbot.Execution
{
    /// <summary>
    /// This helper class wraps a source and handles all input and output parameter validation as well as ensuring methods not called out of order.
    /// </summary>
    public sealed class SourceGuards : IRowSource, IDisposable
    {
        private bool Initialized { get; set; } = false;
        private bool Completed { get; set; } = false;
        private int _columnCount = -1;
        private bool? _previousReadResult = null;
        private readonly IRowSource _rowSource;

        public SourceGuards(IRowSource rowSource)
        {
            _rowSource = rowSource ?? throw new ArgumentNullException(nameof(rowSource));
        }

        public ColumnInfo[] InitAndGetColumns()
        {
            if (Initialized)
                throw new InvalidOperationException("Already initialized");
            Initialized = true;
            var columns = _rowSource.InitAndGetColumns();
            if (columns == null)
                throw new InvalidOperationException("Null was returned by OnInitAndGetColumns() which is not valid.");
            if (columns.Any(c => c == null))
                throw new InvalidOperationException("Returned columns array from OnInitAndGetColumns() contains one or more null values which is not allowed.");

            _columnCount = columns.Length;
            return columns;
        }

        public bool ReadRow(object[] values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (!Initialized)
                throw new InvalidOperationException("Not initialized");

            if (values.Length != _columnCount)
                throw new InvalidOperationException($"Provided values object[] buffer contains {values.Length} slots, but column count returned earlier container {_columnCount} columns. These counts must match.");

            if (_previousReadResult == false)
                throw new InvalidOperationException("It is not allowed to call Read after the method has already returned false in a previous call.");

            var readResult = _rowSource.ReadRow(values);
            _previousReadResult = readResult;
            return readResult;
        }

        public void Complete()
        {
            if (!Initialized)
                throw new InvalidOperationException("Not initialized");
            if (Completed)
                throw new InvalidOperationException("Already completed");
            Completed = true;
            _rowSource.Complete();
        }

        public void Dispose()
        {
            (_rowSource as IDisposable)?.Dispose();
        }
    }
}
