using Rowbot.Execution;

namespace Rowbot.Test.Execution
{
    public class AsyncEnumerableTargetGuards_Test
    {
        [Fact]
        public async Task CallingWriteRowAfterCompleted_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new AsyncEnumerableTargetGuards<int>(target);

            await guard.InitAsync(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            var result1 = await guard.WriteRowAsync(new object[] { "Hello there æå 1", -12.45m, 42 });
            var result2 = await guard.WriteRowAsync(new object[] { "Hello there æå 1", -12.45m, 42 });

            Assert.Equal(1001, result1);
            Assert.Equal(1002, result2);
            await guard.CompleteAsync();

            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => guard.WriteRowAsync(new object[] { "Hello there æå 1", -12.45m, 42 }));

            Assert.Equal(2, target.OnWriteRowCallCount);
        }

        [Fact]
        public async Task CallingWriteRow_NullValues_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new AsyncEnumerableTargetGuards<int>(target);

            await guard.InitAsync(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            await guard.WriteRowAsync(new object[] { "Hello there æå 1", -12.45m, 42 });
            await guard.WriteRowAsync(new object[] { "Hello there æå 1", -12.45m, 42 });
            await guard.WriteRowAsync(new object[] { "Hello there æå 1", -12.45m, 42 });
            await guard.WriteRowAsync(new object[] { "Hello there æå 1", -12.45m, 42 });
            Assert.Equal(4, target.OnWriteRowCallCount);

            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => guard.WriteRowAsync(null));
            Assert.Equal(4, target.OnWriteRowCallCount);

            // Ensure possible to call with non-values afterwards
            await guard.WriteRowAsync(new object[] { "Hello there æå 1", -12.45m, 42 });
            await guard.WriteRowAsync(new object[] { "Hello there æå 1", -12.45m, 42 });

            Assert.Equal(6, target.OnWriteRowCallCount);
        }

        [Fact]
        public async Task Call_WriteRowBeforeCallingInit_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new AsyncEnumerableTargetGuards<int>(target);

            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => guard.WriteRowAsync(new object[] { "Hello there æå 1", -12.45m, 42 }));
            Assert.Equal(0, target.OnWriteRowCallCount);
        }

        [Fact]
        public async Task Call_InitTwice_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new AsyncEnumerableTargetGuards<int>(target);

            await guard.InitAsync(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => guard.InitAsync(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            }));

            Assert.Equal(1, target.OnInitCallCount);
        }

        [Fact]
        public async Task Call_Init_WithNull_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new AsyncEnumerableTargetGuards<int>(target);

            _ = await Assert.ThrowsAsync<ArgumentNullException>(() => guard.InitAsync(null));
            Assert.Equal(0, target.OnInitCallCount);

            // Ensure possible to call init once afterwards
            await guard.InitAsync(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });
            Assert.Equal(1, target.OnInitCallCount);
        }

        [Fact]
        public async Task Call_CompleteBeforeInit_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new AsyncEnumerableTargetGuards<int>(target);

            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => guard.CompleteAsync());

            Assert.Equal(0, target.OnCompleteCallCount);
        }

        [Fact]
        public async Task Call_CompleteTwice_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new AsyncEnumerableTargetGuards<int>(target);

            await guard.InitAsync(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            await guard.CompleteAsync();

            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => guard.CompleteAsync());

            Assert.Equal(1, target.OnCompleteCallCount);
        }

        [Fact]
        public async Task Dispose_EnsureCallingDisposeIfTargetImplementsIDisposable()
        {
            var target = new UnitTestTargetWithDispose();
            var guard = new AsyncEnumerableTargetGuards<int>(target);

            await guard.InitAsync(new ColumnInfo[0]);
            await guard.WriteRowAsync(new object[0]);
            await guard.CompleteAsync();

            Assert.Equal(0, target.DisposeCallCount);

            guard.Dispose();

            Assert.Equal(1, target.DisposeCallCount);

#pragma warning disable S3966 // Objects should not be disposed more than once
            guard.Dispose();
#pragma warning restore S3966 // Objects should not be disposed more than once

            Assert.Equal(2, target.DisposeCallCount);
        }

        private sealed class UnitTestTarget : IAsyncEnumerableRowTarget<int>
        {
            public int OnCompleteCallCount = 0;
            public int OnInitCallCount = 0;
            public int OnWriteRowCallCount = 0;
            public int DisposeCallCount = 0;
            
            public void Dispose()
            {
                DisposeCallCount++;
            }

            public Task CompleteAsync()
            {
                OnCompleteCallCount++;
                return Task.CompletedTask;
            }

            public Task InitAsync(ColumnInfo[] columns)
            {
                OnInitCallCount++;
                return Task.CompletedTask;
            }

            private int _writeCounter = 1000;
            public Task<int> WriteRowAsync(object[] values)
            {
                OnWriteRowCallCount++;
                _writeCounter++;
                return Task.FromResult(_writeCounter);
            }
        }

        private sealed class UnitTestTargetWithDispose : IAsyncEnumerableRowTarget<int>, IDisposable
        {
            public int DisposeCallCount { get; private set; } = 0;
            public void Dispose()
            {
                DisposeCallCount++;
            }

            public Task CompleteAsync()
            {
                return Task.CompletedTask;
            }

            public Task InitAsync(ColumnInfo[] columns)
            {
                return Task.CompletedTask;
            }

            public Task<int> WriteRowAsync(object[] values)
            {
                return Task.FromResult(42);
            }
        }
    }
}
