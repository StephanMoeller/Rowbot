using Rowbot.Core.Execution;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rowbot.Execution
{
    public class RowbotExecutor
    {
        private readonly IRowSource _source;
        private readonly IRowTarget _target;

        public RowbotExecutor(IRowSource source, IRowTarget target)
        {
            _source = new SourceGuards(source);
            _target = new TargetGuards(target);
        }

        public void Execute()
        {
            // Columns
            var columnNames = _source.InitAndGetColumns();
            _target.Init(columns: columnNames);

            // Rows
            var valuesBuffer = new object[columnNames.Length];
            while (_source.ReadRow(valuesBuffer))
            {
                _target.WriteRow(valuesBuffer);
            }

            _source.Complete();
            _target.Complete();
        }
    }

    public class RowbotExecutor<TElement>
    {
        private readonly SourceGuards _source;
        private readonly IEnumerableTargetGuards<TElement> _target;

        public RowbotExecutor(IRowSource source, IEnumerableRowTarget<TElement> target)
        {
            _source = new SourceGuards(source);
            _target = new IEnumerableTargetGuards<TElement>(target);
        }

        public IEnumerable<TElement> CreateEnumerable()
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
        }
    }
}
