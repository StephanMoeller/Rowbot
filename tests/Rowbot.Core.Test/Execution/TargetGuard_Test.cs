using Rowbot.Core.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Core.Test.Execution
{
    public class TargetGuard_Test
    {
        [Fact]
        public void CallingWriteRowAfterCompleted_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new TargetGuards(target);

            guard.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            guard.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });
            guard.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });

            guard.Complete();

            Assert.Throws<InvalidOperationException>(() => guard.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 }));

            Assert.Equal(2, target.OnWriteRowCallCount);
        }

        [Fact]
        public void CallingWriteRow_NullValues_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new TargetGuards(target);
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
            var guard = new TargetGuards(target);

            Assert.Throws<InvalidOperationException>(() => guard.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 }));
            Assert.Equal(0, target.OnWriteRowCallCount);
        }

        [Fact]
        public void Call_InitTwice_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new TargetGuards(target);

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
            var guard = new TargetGuards(target);

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
            var guard = new TargetGuards(target);

            Assert.Throws<InvalidOperationException>(() => guard.Complete());

            Assert.Equal(0, target.OnCompleteCallCount);
        }

        [Fact]
        public void Call_CompleteTwice_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            var guard = new TargetGuards(target);

            guard.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            guard.Complete();

            Assert.Throws<InvalidOperationException>(() => guard.Complete());

            Assert.Equal(1, target.OnCompleteCallCount);
        }
    }

    public class UnitTestTarget : IRowTarget
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

        public void WriteRow(object[] values)
        {
            OnWriteRowCallCount++;
        }
    }
}
