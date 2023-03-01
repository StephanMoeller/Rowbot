using Rowbot.Core.Execution;
using System;
using System.Collections.Concurrent;
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
}
