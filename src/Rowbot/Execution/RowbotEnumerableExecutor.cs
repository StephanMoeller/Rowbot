using System;
using System.Collections.Generic;

namespace Rowbot.Execution
{
    public sealed class RowbotEnumerableExecutor<TElement> : IDisposable
    {
        private readonly Action<IEnumerable<TElement>> _consumer;
        private readonly SourceGuards _source;
        private readonly EnumerableTargetGuards<TElement> _target;

        public RowbotEnumerableExecutor(IRowSource source, IEnumerableRowTarget<TElement> target, Action<IEnumerable<TElement>> consumer)
        {
            _consumer = consumer;
            _source = new SourceGuards(source);
            _target = new EnumerableTargetGuards<TElement>(target);
        }

        public void Execute()
        {
            _consumer(ExecuteInternal());
        }

        private IEnumerable<TElement> ExecuteInternal()
        {
            // Columns
            var columnNames = _source.InitAndGetColumns();
            _target.Init(columns: columnNames);

            // Rows
            var valuesBuffer = new object[columnNames.Length];
            while (_source.ReadRow(valuesBuffer))
            {
                yield return _target.WriteRow(valuesBuffer);
            }

            _source.Complete();
            _target.Complete();

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