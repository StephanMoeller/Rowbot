using Rowbot.Execution;
using Task = System.Threading.Tasks.Task;

namespace Rowbot.Test.Execution
{
    public class RowbotAsyncExecutor_Test
    {
        [Fact]
        public async Task ExecuteAsync_Test()
        {
            var source = new DisposableSource();
            var target = new DisposableTarget();
            var executor = new RowbotAsyncExecutor(source: source, target: target);

            await executor.ExecuteAsync();

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

        private sealed class DisposableTarget : IAsyncRowTarget, IDisposable
        {
            public bool Completed { get; private set; } = false;
            public bool Disposed { get; private set; } = false;
            public List<object> CallValues { get; set; } = new List<object>();

            public Task CompleteAsync()
            {
                EnsureNotCompletedOrDisposed();
                Completed = true;
                return Task.CompletedTask;
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

            public Task InitAsync(ColumnInfo[] columns)
            {
                CallValues.Add(columns.ToArray());
                return Task.CompletedTask;
            }

            public Task WriteRowAsync(object[] values)
            {
                CallValues.Add(values.ToArray());
                return Task.CompletedTask;
            }
        }

        private sealed class DisposableSource : IAsyncRowSource, IDisposable
        {
            public bool Completed { get; private set; } = false;
            public bool Disposed { get; private set; } = false;

            public DisposableSource()
            {
            }

            public Task CompleteAsync()
            {
                EnsureNotCompletedOrDisposed();
                Completed = true;
                return Task.CompletedTask;
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

            public Task<ColumnInfo[]> InitAndGetColumnsAsync()
            {
                EnsureNotCompletedOrDisposed();
                return Task.FromResult(new ColumnInfo[]
                {
                    new ColumnInfo(name: "ColA", valueType: typeof(string)),
                    new ColumnInfo(name: "ColB", valueType: typeof(int))
                });
            }

            private int _rowCounter = 0;
            public Task<bool> ReadRowAsync(object[] values)
            {
                EnsureNotCompletedOrDisposed();
                _rowCounter++;
                if (_rowCounter > 3)
                    return Task.FromResult(false);

                Assert.Equal(2, values.Length);
                values[0] = "Hello" + _rowCounter;
                values[1] = _rowCounter;
                return Task.FromResult(true);
            }
        }
    }
}