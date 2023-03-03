using Rowbot.Core.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Test.Core.Targets
{
    public class DynamicObjectTarget_Test
    {
        [Fact]
        public void NoColumns_Test()
        {
            using (var target = new DynamicObjectTarget())
            {
                target.Init(new ColumnInfo[0]);

                var e = target.WriteRow(new object[0]);
                Assert.NotNull(e);

                target.Complete();
            }
        }

        [Fact]
        public void NoRows_Test()
        {
            using (var target = new DynamicObjectTarget())
            {
                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "A", typeof(string))
                });

                target.Complete();
            }
        }

        [Fact]
        public void WithRowsAndColumns_Test()
        {
            using (var target = new DynamicObjectTarget())
            {
                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "A", typeof(string)),
                    new ColumnInfo(name: "B", typeof(int)),
                    new ColumnInfo(name: "SomeOtherProperty", typeof(int?))
                });

                {
                    var e = target.WriteRow(new object[] { "Hello", 42, 82 });
                    Assert.Equal("Hello", e.A);
                    Assert.Equal(42, e.B);
                    Assert.Equal(82, e.SomeOtherProperty);
                }

                {
                    var e = target.WriteRow(new object?[] { null, 42, null });
                    Assert.Equal(null, e.A);
                    Assert.Equal(42, e.B);
                    Assert.Equal(null, e.SomeOtherProperty);
                }
            }
        }
    }
}
