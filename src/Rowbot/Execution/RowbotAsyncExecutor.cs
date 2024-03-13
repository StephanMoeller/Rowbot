using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rowbot.Execution
{
    public sealed class RowbotAsyncExecutor : IDisposable
    {
        private readonly AsyncSourceGuards _source;
        private readonly AsyncTargetGuards _target;

        public RowbotAsyncExecutor(IAsyncRowSource source, IAsyncRowTarget target)
        {
            _source = new AsyncSourceGuards(source);
            _target = new AsyncTargetGuards(target);
        }

        public void Dispose()
        {
#pragma warning disable S2486 // Generic exceptions should not be ignored
#pragma warning disable S108 // Nested blocks of code should not be left empty
            try
            {
                _source.Dispose();
            }
            catch { }

            try
            {
                _target.Dispose();
            }
            catch { }
#pragma warning restore S108 // Nested blocks of code should not be left empty
#pragma warning restore S2486 // Generic exceptions should not be ignored
        }

        public async Task ExecuteAsync()
        {
            // Columns
            var columnNames = await _source.InitAndGetColumnsAsync();
            await _target.InitAsync(columns: columnNames);

            // Rows
            var valuesBuffer = new object[columnNames.Length];
            while (await _source.ReadRowAsync(valuesBuffer))
            {
                await _target.WriteRowAsync(valuesBuffer);
            }

            await _source.CompleteAsync();
            await _target.CompleteAsync();

            Dispose();
        }
    }

    public sealed class RowbotAsyncEnumerableExecutor<TElement> : IDisposable
    {
        private readonly AsyncSourceGuards _source;
        private readonly AsyncEnumerableTargetGuards<TElement> _target;

        public RowbotAsyncEnumerableExecutor(IAsyncRowSource source, IAsyncEnumerableRowTarget<TElement> target)
        {
            _source = new AsyncSourceGuards(source);
            _target = new AsyncEnumerableTargetGuards<TElement>(target);
        }

        public Task ExecuteAsync(Func<IAsyncEnumerable<TElement>, Task> consumer)
        {
            return consumer(ExecuteInternal());
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
            catch { }

            try
            {
                _target.Dispose();
            }
            catch { }
#pragma warning restore S108 // Nested blocks of code should not be left empty
#pragma warning restore S2486 // Generic exceptions should not be ignored
        }
    }
}
