using Rowbot.Execution;

namespace Rowbot.Test.Execution
{
    public class AsyncSourceGuards_Test
    {
        [Fact]
        public async Task Call_InitAndGetColumns_EnsureArrayPassedThrough()
        {
            var source = new UnitTestSource();
            var guard = new AsyncSourceGuards(source);
            Assert.Equal(0, source.OnInitCallCount);

            var columns = await guard.InitAndGetColumnsAsync();
            Assert.Same(source.Columns, columns);
            Assert.Equal(1, source.OnInitCallCount);
        }

        [Fact]
        public async Task Call_InitAndGetColumns_EmptyColumnArray_ExpectAllowed()
        {
            var emptyColumnArray = new ColumnInfo[0];

            var source = new UnitTestSource();
            var guard = new AsyncSourceGuards(source);
            source.Columns = emptyColumnArray;

            Assert.Equal(0, source.OnInitCallCount);

            var columns = await guard.InitAndGetColumnsAsync();
            Assert.Same(emptyColumnArray, columns);
        }

        [Fact]
        public async Task Call_InitTwice_ExpectException()
        {
            var source = new UnitTestSource();
            var guard = new AsyncSourceGuards(source);
            Assert.Equal(0, source.OnInitCallCount);

            _ = await guard.InitAndGetColumnsAsync();
            Assert.Equal(1, source.OnInitCallCount);
            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => guard.InitAndGetColumnsAsync());
            Assert.Equal(1, source.OnInitCallCount);
        }

        [Fact]
        public async Task Call_CompleteBeforeInit_ExpectException()
        {
            var source = new UnitTestSource();
            var guard = new AsyncSourceGuards(source);
            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => guard.CompleteAsync());
            Assert.Equal(0, source.OnCompleteCallCount);
        }

        [Fact]
        public async Task Call_CompleteMultipleTimes_ExpectException()
        {
            var source = new UnitTestSource();
            var guard = new AsyncSourceGuards(source);
            _ = await guard.InitAndGetColumnsAsync();
            await guard.CompleteAsync();
            Assert.Equal(1, source.OnCompleteCallCount);

            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => guard.CompleteAsync());
            Assert.Equal(1, source.OnCompleteCallCount);
        }

        [Fact]
        public async Task Successful_Run_AssertAllMethodsCalled()
        {
            var source = new UnitTestSource();
            var guard = new AsyncSourceGuards(source);

            Assert.Equal(0, source.OnInitCallCount);
            Assert.Equal(0, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            var colums = await guard.InitAndGetColumnsAsync();

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(0, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            Assert.True(await guard.ReadRowAsync(new object[colums.Length]));

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(1, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            Assert.True(await guard.ReadRowAsync(new object[colums.Length]));

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(2, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            Assert.True(await guard.ReadRowAsync(new object[colums.Length]));

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(3, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            source.SetNextReadReturnValue(false);

            Assert.False(await guard.ReadRowAsync(new object[colums.Length]));

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(4, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            await guard.CompleteAsync();

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(4, source.OnReadRowCallCount);
            Assert.Equal(1, source.OnCompleteCallCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task ReadCalledWithWrongArraySize_ExpectExceptions(int arraySize)
        {
            var source = new UnitTestSource();
            var guard = new AsyncSourceGuards(source);

            var columns = await guard.InitAndGetColumnsAsync();

            Assert.NotEqual(columns.Length, 
                arraySize); // Sanity testing that we don't test with the actual expected array size
            Assert.True(await source.ReadRowAsync(new object[columns.Length]));
            _ = await Assert.ThrowsAsync<InvalidOperationException>(() => guard.ReadRowAsync(new object[arraySize]));
        }

        [Fact]
        public async Task Dispose_EnsureCallingDisposeIfSourceImplementsIDisposable()
        {
            var source = new UnitTestSourceWithDispose();
            var guard = new AsyncSourceGuards(source);

            await guard.InitAndGetColumnsAsync();
            await guard.ReadRowAsync(new object[0]);

            await guard.CompleteAsync();

            Assert.Equal(0, source.DisposeCallCount);

            guard.Dispose();

            Assert.Equal(1, source.DisposeCallCount);

#pragma warning disable S3966 // Objects should not be disposed more than once
            guard.Dispose();
#pragma warning restore S3966 // Objects should not be disposed more than once

            Assert.Equal(2, source.DisposeCallCount);
        }

        private class UnitTestSource : IAsyncRowSource
        {
            public int OnCompleteCallCount = 0;
            public int OnInitCallCount = 0;
            public int OnReadRowCallCount = 0;

            public ColumnInfo[] Columns = new ColumnInfo[]
                { new ColumnInfo("col1", typeof(string)), new ColumnInfo("col2", typeof(int)) };

            private bool _nextReadReturnValue = true;

            public void SetNextReadReturnValue(bool nextReadReturnValue)
            {
                _nextReadReturnValue = nextReadReturnValue;
            }

            public Task CompleteAsync()
            {
                OnCompleteCallCount++;
                return Task.CompletedTask;
            }

            public Task<ColumnInfo[]> InitAndGetColumnsAsync()
            {
                OnInitCallCount++;
                return Task.FromResult(Columns);
            }

            public Task<bool> ReadRowAsync(object[] values)
            {
                OnReadRowCallCount++;
                return Task.FromResult(_nextReadReturnValue);
            }
        }

        private sealed class UnitTestSourceWithDispose : IAsyncRowSource, IDisposable
        {
            public int DisposeCallCount { get; private set; } = 0;

            public Task CompleteAsync()
            {
                return Task.CompletedTask;
            }

            public Task<ColumnInfo[]> InitAndGetColumnsAsync()
            {
                return Task.FromResult(new ColumnInfo[0]);
            }

            public Task<bool> ReadRowAsync(object[] values)
            {
                return Task.FromResult(true);
            }

            public void Dispose()
            {
                DisposeCallCount++;
            }
        }
    }
}