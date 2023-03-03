using Rowbot.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Test.Core.Execution
{
    public class RowbotExecutor_Test
    {
        [Fact]
        public void Execute_Test()
        {
            var source = new DisposableSource();
            var target = new DisposableTarget();
            var executor = new RowbotExecutor(source: source, target: target);

            executor.Execute();

            Assert.True(source.Completed);
            Assert.True(target.Completed);

            Assert.True(source.Disposed);
            Assert.True(target.Disposed);

            Assert.Equal(4, target.CallValues.Count);
            var columns = (ColumnInfo[])target.CallValues[0];
            Assert.Equal(2, columns.Length);

            // Assert columns
            Assert.Equal("ColA", columns[0].Name);
            Assert.Equal(typeof(string), columns[0].ValueType);

            Assert.Equal("ColB", columns[1].Name);
            Assert.Equal(typeof(int), columns[1].ValueType);

            // Assert data
            {
                var row = (object[])target.CallValues[1];
                Assert.Equal(2, row.Length);
                Assert.Equal("Hello1", row[0]);
                Assert.Equal(1, row[1]);
            }

            {
                var row = (object[])target.CallValues[2];
                Assert.Equal(2, row.Length);
                Assert.Equal("Hello2", row[0]);
                Assert.Equal(2, row[1]);
            }

            {
                var row = (object[])target.CallValues[3];
                Assert.Equal(2, row.Length);
                Assert.Equal("Hello3", row[0]);
                Assert.Equal(3, row[1]);
            }
        }
    }

    public sealed class DisposableTarget : IRowTarget, IDisposable
    {
        public bool Completed { get; private set; } = false;
        public bool Disposed { get; private set; } = false;
        public List<object> CallValues { get; set; } = new List<object>();

        public void Complete()
        {
            EnsureNotCompletedOrDisposed();
            Completed = true;
        }

        private void EnsureNotCompletedOrDisposed()
        {
            Assert.False(Completed);
            Assert.False(Disposed);
        }

        public void Dispose()
        {
            Disposed = true;
        }

        public void Init(ColumnInfo[] columns)
        {
            CallValues.Add(columns.ToArray());
        }

        public void WriteRow(object[] values)
        {
            CallValues.Add(values.ToArray());
        }
    }

    public sealed class DisposableSource : IRowSource, IDisposable
    {
        public bool Completed { get; private set; } = false;
        public bool Disposed { get; private set; } = false;
        public DisposableSource()
        {

        }

        public void Complete()
        {
            EnsureNotCompletedOrDisposed();
            Completed = true;
        }

        private void EnsureNotCompletedOrDisposed()
        {
            Assert.False(Completed);
            Assert.False(Disposed);
        }

        public void Dispose()
        {
            Disposed = true;
        }

        public ColumnInfo[] InitAndGetColumns()
        {
            EnsureNotCompletedOrDisposed();
            return new ColumnInfo[] {
                new ColumnInfo(name: "ColA", valueType: typeof(string)),
                new ColumnInfo(name: "ColB", valueType: typeof(int))
            };

        }

        private int _rowCounter = 0;
        public bool ReadRow(object[] values)
        {
            EnsureNotCompletedOrDisposed();
            _rowCounter++;
            if (_rowCounter > 3)
                return false;

            Assert.Equal(2, values.Length);
            values[0] = "Hello" + _rowCounter;
            values[1] = _rowCounter;
            return true;
        }
    }
}

