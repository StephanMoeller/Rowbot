using Rowbot.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Test
{
    public class RowSource_Test
    {
        [Fact]
        public void Call_InitAndGetColumns_EnsureArrayPassedThrough()
        {
            var source = new UnitTestSource();
            Assert.Equal(0, source.OnInitCallCount);

            var columns = source.InitAndGetColumns();
            Assert.Same(source.Columns, columns);
            Assert.Equal(1, source.OnInitCallCount);
        }

        [Fact]
        public void Call_InitAndGetColumns_EmptyColumnArray_ExpectAllowed()
        {
            var emptyColumnArray = new ColumnInfo[0];

            var source = new UnitTestSource();
            source.Columns = emptyColumnArray;

            Assert.Equal(0, source.OnInitCallCount);

            var columns = source.InitAndGetColumns();
            Assert.Same(emptyColumnArray, columns);
        }

        [Fact]
        public void Call_InitTwice_ExpectException()
        {
            var source = new UnitTestSource();
            Assert.Equal(0, source.OnInitCallCount);

            source.InitAndGetColumns();
            Assert.Equal(1, source.OnInitCallCount);
            Assert.Throws<InvalidOperationException>(() => source.InitAndGetColumns());
            Assert.Equal(1, source.OnInitCallCount);
        }

        [Fact]
        public void Call_CompleteBeforeInit_ExpectException()
        {
            var source = new UnitTestSource();
            Assert.Throws<InvalidOperationException>(() => source.Complete());
            Assert.Equal(0, source.OnCompleteCallCount);
        }

        [Fact]
        public void Call_CompleteMultipleTimes_ExpectException()
        {
            var source = new UnitTestSource();
            source.InitAndGetColumns();
            source.Complete();
            Assert.Equal(1, source.OnCompleteCallCount);

            Assert.Throws<InvalidOperationException>(() => source.Complete());
            Assert.Equal(1, source.OnCompleteCallCount);
        }

        [Fact]
        public void Successful_Run_AssertAllMethodsCalled()
        {
            var source = new UnitTestSource();

            Assert.Equal(0, source.OnInitCallCount);
            Assert.Equal(0, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            var colums = source.InitAndGetColumns();

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(0, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            Assert.True(source.ReadRow(new object[colums.Length]));

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(1, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            Assert.True(source.ReadRow(new object[colums.Length]));

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(2, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            Assert.True(source.ReadRow(new object[colums.Length]));

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(3, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            source.SetNextReadReturnValue(false);

            Assert.False(source.ReadRow(new object[colums.Length]));

            Assert.Equal(1, source.OnInitCallCount);
            Assert.Equal(4, source.OnReadRowCallCount);
            Assert.Equal(0, source.OnCompleteCallCount);

            source.Complete();

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

            var columns = source.InitAndGetColumns();

            Assert.NotEqual(columns.Length, arraySize);// Sanity testing that we don't test with the actual expected array size
            Assert.True(source.ReadRow(new object[columns.Length]));
            Assert.Throws<InvalidOperationException>(() => source.ReadRow(new object[arraySize]));
        }
    }


    public class UnitTestSource : RowSource
    {
        public int OnCompleteCallCount = 0;
        public int OnInitCallCount = 0;
        public int OnReadRowCallCount = 0;
        public ColumnInfo[] Columns = new ColumnInfo[] { new ColumnInfo("col1", typeof(string)), new ColumnInfo("col2", typeof(int)) };
        private bool _nextReadReturnValue = true;

        public void SetNextReadReturnValue(bool nextReadReturnValue)
        {
            _nextReadReturnValue = nextReadReturnValue;
        }

        public override void Dispose()
        {

        }

        protected override void OnComplete()
        {
            OnCompleteCallCount++;
        }

        protected override ColumnInfo[] OnInitAndGetColumns()
        {
            OnInitCallCount++;
            return Columns;
        }

        protected override bool OnReadRow(object[] values)
        {
            OnReadRowCallCount++;
            return _nextReadReturnValue;
        }
    }
}
