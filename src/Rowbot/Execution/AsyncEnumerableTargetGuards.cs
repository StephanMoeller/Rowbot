using System;
using System.Threading.Tasks;

namespace Rowbot.Execution
{
    public sealed class AsyncEnumerableTargetGuards<T> : IAsyncEnumerableRowTarget<T>, IDisposable
    {
        private readonly IAsyncEnumerableRowTarget<T> _rowTarget;

        private bool Completed { get; set; } = false;
        private bool Initialized { get; set; } = false;
        
        public AsyncEnumerableTargetGuards(IAsyncEnumerableRowTarget<T> rowTarget)
        {
            _rowTarget = rowTarget ?? throw new ArgumentNullException(nameof(rowTarget));
        }

        public Task CompleteAsync()
        {
            if (!Initialized)
                throw new InvalidOperationException("Init must be called before Complete()");
            if (Completed)
                throw new InvalidOperationException("Complete already called and can only be called once.");
            Completed = true;
            
            return _rowTarget.CompleteAsync();
        }

        public void Dispose()
        {
            (_rowTarget as IDisposable)?.Dispose();
        }

        public Task InitAsync(ColumnInfo[] columns)
        {
            if (columns is null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            if (Initialized)
                throw new InvalidOperationException("Init has already been called and can only be called once.");
            Initialized = true;
            
            return _rowTarget.InitAsync(columns);
        }

        public Task<T> WriteRowAsync(object[] values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (!Initialized)
                throw new InvalidOperationException("Init must be called before WriteRows");
            if (Completed)
                throw new InvalidOperationException("Complete already called. Not allowed to write more rows");
            
            return _rowTarget.WriteRowAsync(values);
        }
    }
}
