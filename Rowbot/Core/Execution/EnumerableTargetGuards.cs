using System;
using System.Collections.Generic;
using System.Text;

namespace Rowbot.Core.Execution
{
    public sealed class EnumerableTargetGuards<T> : IEnumerableRowTarget<T>, IDisposable
    {
        private readonly IEnumerableRowTarget<T> _rowTarget;

        private bool Completed { get; set; } = false;
        private bool Initialized { get; set; } = false;
        public EnumerableTargetGuards(IEnumerableRowTarget<T> rowTarget)
        {
            _rowTarget = rowTarget ?? throw new ArgumentNullException(nameof(rowTarget));
        }

        public void Complete()
        {
            if (!Initialized)
                throw new InvalidOperationException("Init must be called before Complete()");
            if (Completed)
                throw new InvalidOperationException("Complete already called and can only be called once.");
            Completed = true;
            _rowTarget.Complete();
        }

        public void Dispose()
        {
            (_rowTarget as IDisposable)?.Dispose();
        }

        public void Init(ColumnInfo[] columns)
        {
            if (columns is null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            if (Initialized)
                throw new InvalidOperationException("Init has already been called and can only be called once.");
            Initialized = true;
            _rowTarget.Init(columns);
        }

        public T WriteRow(object[] values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (!Initialized)
                throw new InvalidOperationException("Init must be called before WriteRows");
            if (Completed)
                throw new InvalidOperationException("Complete already called. Not allowed to write more rows");
            return _rowTarget.WriteRow(values);
        }
    }
}
