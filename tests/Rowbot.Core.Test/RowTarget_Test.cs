using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Test
{
    public class RowTarget_Test
    {
        [Fact]
        public void CallingWriteRowAfterCompleted_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            target.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            target.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });
            target.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });

            target.Complete();

            Assert.Throws<InvalidOperationException>(() => target.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 }));

            Assert.Equal(2, target.OnWriteRowCallCount);
        }

        [Fact]
        public void CallingWriteRow_NullValues_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            target.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            target.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });
            target.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });
            target.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });
            target.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });
            Assert.Equal(4, target.OnWriteRowCallCount);

            Assert.Throws<ArgumentNullException>(() => target.WriteRow(null));
            Assert.Equal(4, target.OnWriteRowCallCount);

            // Ensure possible to call with non-values afterwards
            target.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });
            target.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 });

            Assert.Equal(6, target.OnWriteRowCallCount);
        }

        [Fact]
        public void Call_WriteRowBeforeCallingInit_ExpectException_Test()
        {
            var target = new UnitTestTarget();

            Assert.Throws<InvalidOperationException>(() => target.WriteRow(new object[] { "Hello there æå 1", -12.45m, 42 }));
            Assert.Equal(0, target.OnWriteRowCallCount);
        }

        [Fact]
        public void Call_InitTwice_ExpectException_Test()
        {
            var target = new UnitTestTarget();

            target.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            Assert.Throws<InvalidOperationException>(() => target.Init(new ColumnInfo[]{
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

            Assert.Throws<ArgumentNullException>(() => target.Init(null));
            Assert.Equal(0, target.OnInitCallCount);

            // Ensure possible to call init once afterwards
            target.Init(new ColumnInfo[]{
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

            Assert.Throws<InvalidOperationException>(() => target.Complete());

            Assert.Equal(0, target.OnCompleteCallCount);
        }

        [Fact]
        public void Call_CompleteTwice_ExpectException_Test()
        {
            var target = new UnitTestTarget();
            target.Init(new ColumnInfo[]{
                new ColumnInfo(name: "Col1", valueType: typeof(string)),
                new ColumnInfo(name: "Col2", valueType: typeof(decimal)),
                new ColumnInfo(name: "Col æøå 3", valueType: typeof(int)),
            });

            target.Complete();

            Assert.Throws<InvalidOperationException>(() => target.Complete());

            Assert.Equal(1, target.OnCompleteCallCount);
        }
    }

    public class UnitTestTarget : RowTarget
    {
        public int OnCompleteCallCount = 0;
        public int OnInitCallCount = 0;
        public int OnWriteRowCallCount = 0;
        public int DispoeCallCount = 0;
        public override void Dispose()
        {
            DispoeCallCount++;
        }

        protected override void OnComplete()
        {
            OnCompleteCallCount++;
        }

        protected override void OnInit(ColumnInfo[] columns)
        {
            OnInitCallCount++;
        }

        protected override void OnWriteRow(object[] values)
        {
            OnWriteRowCallCount++;
        }
    }
}
