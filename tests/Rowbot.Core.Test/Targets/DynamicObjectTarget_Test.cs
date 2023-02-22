using Rowbot.Core.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Test.Targets
{
    public class DynamicObjectTarget_Test
    {
        [Fact]
        public void NoColumns_Test()
        {
            using (var target = new DynamicObjectTarget())
            {
                target.Init(new ColumnInfo[0]);

                target.WriteRow(new object[0]);

                target.Complete();

                var result = target.GetResult();
                Assert.Single(result);
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

                Assert.Empty(target.GetResult());
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

                target.WriteRow(new object[] { "Hello", 42, 82 });
                target.WriteRow(new object?[] { null, 42, null });

                target.Complete();

                var result = target.GetResult().ToArray();
                Assert.Equal(2, result.Length);

                {
                    var e = result[0];
                    Assert.Equal("Hello", e.A);
                    Assert.Equal(42, e.B);
                    Assert.Equal(82, e.SomeOtherProperty);
                }

                {
                    var e = result[1];
                    Assert.Equal(null, e.A);
                    Assert.Equal(42, e.B);
                    Assert.Equal(null, e.SomeOtherProperty);
                }
            }
        }
    }
}
