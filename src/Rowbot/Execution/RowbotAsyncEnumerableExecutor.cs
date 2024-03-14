using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rowbot.Execution
{
    public sealed class RowbotAsyncEnumerableExecutor<TElement> : IDisposable
    {
        private readonly Func<IAsyncEnumerable<TElement>, Task> _consumer;
        private readonly AsyncSourceGuards _source;
        private readonly AsyncEnumerableTargetGuards<TElement> _target;

        public RowbotAsyncEnumerableExecutor(IAsyncRowSource source, IAsyncEnumerableRowTarget<TElement> target, Func<IAsyncEnumerable<TElement>, Task> consumer)
        {
            _consumer = consumer;
            _source = new AsyncSourceGuards(source);
            _target = new AsyncEnumerableTargetGuards<TElement>(target);
        }

        public Task ExecuteAsync()
        {
            return _consumer(ExecuteInternal());
        }

        private async IAsyncEnumerable<TElement> ExecuteInternal()
        {
            // Columns
            var columnNames = await _source.InitAndGetColumnsAsync();
            await _target.InitAsync(columns: columnNames);

            // Rows
            var valuesBuffer = new object[columnNames.Length];
            while (await _source.ReadRowAsync(valuesBuffer))
            {
                yield return await _target.WriteRowAsync(valuesBuffer);
            }

            await _source.CompleteAsync();
            await _target.CompleteAsync();

            Dispose();
        }

        public void Dispose()
        {
#pragma warning disable S2486 // Generic exceptions should not be ignored
#pragma warning disable S108 // Nested blocks of code should not be left empty
            try
            {
                _source.Dispose();
            }
            catch
            {
            }

            try
            {
                _target.Dispose();
            }
            catch
            {
            }
#pragma warning restore S108 // Nested blocks of code should not be left empty
#pragma warning restore S2486 // Generic exceptions should not be ignored
        }
    }
}