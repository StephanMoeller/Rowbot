using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rowbot
{
    public abstract class RowSource : IDisposable
    {
        protected bool Initialized { get; private set; } = false;
        protected bool Completed { get; private set; } = false;
        protected int _columnCount = -1;
        private bool? _previousReadResult = null;

        public ColumnInfo[] InitAndGetColumns()
        {
            if (Initialized)
                throw new InvalidOperationException("Already initialized");
            Initialized = true;
            var columns = OnInitAndGetColumns();
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

            var readResult = OnReadRow(values);
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
            OnComplete();
        }

        protected abstract ColumnInfo[] OnInitAndGetColumns();
        protected abstract bool OnReadRow(object[] values);
        public abstract void Dispose();
        protected abstract void OnComplete();
    }
}
