using Rowbot.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Rowbot.Test.Execution.TargetGuard_Test;

namespace Rowbot.Test.Execution
{
    public class EnumerableTargetGuards_Test
    {
        [Fact]
        public void CallingWriteRowAfterCompleted_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new EnumerableTargetGuards<int>(target);

            guard.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            var result1 = guard.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });
            var result2 = guard.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });

            Assert.Equal(1001, result1);
            Assert.Equal(1002, result2);
            guard.Complete();

            Assert.Throws<InvalidOperationException>(() => guard.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 }));

            Assert.Equal(2, target.OnWriteRowCallCount);
        }

        [Fact]
        public void CallingWriteRow_NullValues_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new EnumerableTargetGuards<int>(target);

            guard.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            guard.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });
            guard.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });
            guard.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });
            guard.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });
            Assert.Equal(4, target.OnWriteRowCallCount);

            Assert.Throws<ArgumentNullException>(() => guard.WriteRow(null));
            Assert.Equal(4, target.OnWriteRowCallCount);

            // Ensure possible to call with non-values afterwards
            guard.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });
            guard.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });

            Assert.Equal(6, target.OnWriteRowCallCount);
        }

        [Fact]
        public void Call_WriteRowBeforeCallingInit_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new EnumerableTargetGuards<int>(target);

            Assert.Throws<InvalidOperationException>(() => guard.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 }));
            Assert.Equal(0, target.OnWriteRowCallCount);
        }

        [Fact]
        public void Call_InitTwice_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new EnumerableTargetGuards<int>(target);

            guard.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            Assert.Throws<InvalidOperationException>(() => guard.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            }));

            Assert.Equal(1, target.OnInitCallCount);
        }

        [Fact]
        public void Call_Init_WithNull_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new EnumerableTargetGuards<int>(target);

            Assert.Throws<ArgumentNullException>(() => guard.Init(null));
            Assert.Equal(0, target.OnInitCallCount);

            // Ensure possible to call init once afterwards
            guard.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });
            Assert.Equal(1, target.OnInitCallCount);
        }

        [Fact]
        public void Call_CompleteBeforeInit_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new EnumerableTargetGuards<int>(target);

            Assert.Throws<InvalidOperationException>(() => guard.Complete());

            Assert.Equal(0, target.OnCompleteCallCount);
        }

        [Fact]
        public void Call_CompleteTwice_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new EnumerableTargetGuards<int>(target);

            guard.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            guard.Complete();

            Assert.Throws<InvalidOperationException>(() => guard.Complete());

            Assert.Equal(1, target.OnCompleteCallCount);
        }

        [Fact]
        public void Dispose_EnsureCallingDisposeIfTargetImplementsIDisposable()
        {
            var target = new UnitTestTargetWithDispose();
            var guard = new EnumerableTargetGuards<int>(target);

            guard.Init(new ColumnInfo[0]);
            guard.WriteRow(new object[0]);
            guard.Complete();

            Assert.Equal(0, target.DisposeCallCount);

            guard.Dispose();

            Assert.Equal(1, target.DisposeCallCount);

#pragma warning disable S3966 // Objects should not be disposed more than once
            guard.Dispose();
#pragma warning restore S3966 // Objects should not be disposed more than once

            Assert.Equal(2, target.DisposeCallCount);
        }

        public sealed class UnitTestTarget : IEnumerableRowTarget<int>
        {
            public int OnCompleteCallCount = 0;
            public int OnInitCallCount = 0;
            public int OnWriteRowCallCount = 0;
            public int DispoeCallCount = 0;
            public void Dispose()
            {
                DispoeCallCount++;
            }

            public void Complete()
            {
                OnCompleteCallCount++;
            }

            public void Init(ColumnInfo[] columns)
            {
                OnInitCallCount++;
            }

            private int _writeCounter = 1000;
            public int WriteRow(object[] values)
            {
                OnWriteRowCallCount++;
                _writeCounter++;
                return _writeCounter;
            }
        }

        public sealed class UnitTestTargetWithDispose : IEnumerableRowTarget<int>, IDisposable
        {
            public int DisposeCallCount { get; private set; } = 0;
            public void Dispose()
            {
                DisposeCallCount++;
            }

            public void Complete()
            {
                //
            }

            public void Init(ColumnInfo[] columns)
            {
                //
            }

            public int WriteRow(object[] values)
            {
                return 42;
            }
        }
    }
}
