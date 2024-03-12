using Rowbot.Execution;

namespace Rowbot.Test.Execution
{
    public class SourceGuards_Test
    {
        [Fact]
        public void Call_InitAndGetColumns_EnsureArrayPassedThrough()
        {
            var source = new UnitTestSource();
            var guard = new SourceGuards(source);
            Assert.Equal(0, source.OnInitCallCount);

            var columns = guard.InitAndGetColumns();
            Assert.Same(source.Columns, columns);
            Assert.Equal(1, source.OnInitCallCount);
        }

        [Fact]
        public void Call_InitAndGetColumns_EmptyColumnArray_ExpectAllowed()
        {
            var emptyColumnArray = new ColumnInfo[0];

            var source = new UnitTestSource();
            var guard = new SourceGuards(source);
            source.Columns = emptyColumnArray;

            Assert.Equal(0, source.OnInitCallCount);

            var columns = guard.InitAndGetColumns();
            Assert.Same(emptyColumnArray, columns);
        }

        [Fact]
        public void Call_InitTwice_ExpectException()
        {
            var source = new UnitTestSource();
            var guard = new SourceGuards(source);
            Assert.Equal(0, source.OnInitCallCount);

            guard.InitAndGetColumns();
            Assert.Equal(1, source.OnInitCallCount);
            Assert.Throws<InvalidOperationException>(() => guard.InitAndGetColumns());
            Assert.Equal(1, source.OnInitCallCount);
        }

        [Fact]
        public void Call_CompleteBeforeInit_ExpectException()
        {
            var source = new UnitTestSource();
            var guard = new SourceGuards(source);
            Assert.Throws<InvalidOperationException>(() => guard.Complete());
            Assert.Equal(0, source.OnCompleteCallCount);
        }

        [Fact]
        public void Call_CompleteMultipleTimes_ExpectException()
        {
            var source = new UnitTestSource();
            var guard = new SourceGuards(source);
            guard.InitAndGetColumns();
            guard.Complete();
            Assert.Equal(1, source.OnCompleteCallCount);

            Assert.Throws<InvalidOperationException>(() => guard.Complete());
            Assert.Equal(1, source.OnCompleteCallCount);
        }

        [Fact]
        public void Successful_Run_AssertAllMethodsCalled()
        {
            var source = new UnitTestSource();
            var guard = new SourceGuards(source);

            Assert.Equal(0, source.OnInitCallCount);
            Assert.Equal(0, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            var colums = guard.InitAndGetColumns();

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(0, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            Assert.True(guard.ReadRow(new object[colums.Length]));

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(1, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            Assert.True(guard.ReadRow(new object[colums.Length]));

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(2, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            Assert.True(guard.ReadRow(new object[colums.Length]));

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(3, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            source.SetNextReadReturnValue(false);

            Assert.False(guard.ReadRow(new object[colums.Length]));

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(4, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            guard.Complete();

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(4, source.OnReadRowCallCount);
            Assert.Equal(1, source.OnCompleteCallCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(4)]
        public void ReadCalledWithWrongArraySize_ExpectExceptions(int arraySize)
        {
            var source = new UnitTestSource();
            var guard = new SourceGuards(source);

            var columns = guard.InitAndGetColumns();

            Assert.NotEqual(columns.Length,
                arraySize); // Sanity testing that we don't test with the actual expected array size
            Assert.True(source.ReadRow(new object[columns.Length]));
            Assert.Throws<InvalidOperationException>(() => guard.ReadRow(new object[arraySize]));
        }

        [Fact]
        public void Dispose_EnsureCallingDisposeIfSourceImplementsIDisposable()
        {
            var source = new UnitTestSourceWithDispose();
            var guard = new SourceGuards(source);

            guard.InitAndGetColumns();
            guard.ReadRow(new object[0]);

            guard.Complete();

            Assert.Equal(0, source.DisposeCallCount);

            guard.Dispose();

            Assert.Equal(1, source.DisposeCallCount);

#pragma warning disable S3966 // Objects should not be disposed more than once
            guard.Dispose();
#pragma warning restore S3966 // Objects should not be disposed more than once

            Assert.Equal(2, source.DisposeCallCount);
        }

        private class UnitTestSource : IRowSource
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

            public void Complete()
            {
                OnCompleteCallCount++;
            }

            public ColumnInfo[] InitAndGetColumns()
            {
                OnInitCallCount++;
                return Columns;
            }

            public bool ReadRow(object[] values)
            {
                OnReadRowCallCount++;
                return _nextReadReturnValue;
            }
        }

        private sealed class UnitTestSourceWithDispose : IRowSource, IDisposable
        {
            public int DisposeCallCount { get; private set; } = 0;

            public void Complete()
            {
                //
            }

            public ColumnInfo[] InitAndGetColumns()
            {
                return new ColumnInfo[0];
            }

            public bool ReadRow(object[] values)
            {
                return true;
            }

            public void Dispose()
            {
                DisposeCallCount++;
            }
        }
    }
}