using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Rowbot.Execution
{
    public sealed class RowbotExecutor : IDisposable
    {
        private readonly SourceGuards _source;
        private readonly TargetGuards _target;

        public RowbotExecutor(IRowSource source, IRowTarget target)
        {
            _source = new SourceGuards(source);
            _target = new TargetGuards(target);
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

            Dispose();
        }
    }
}
