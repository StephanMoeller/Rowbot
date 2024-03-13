using Rowbot.Targets;

namespace Rowbot.Test.Targets
{
    public class AsyncDynamicObjectTarget_Test
    {
        [Fact]
        public async Task NoColumns_Test()
        {
            using (var target = new AsyncDynamicObjectTarget())
            {
                await target.InitAsync(new ColumnInfo[0]);

                var e = await target.WriteRowAsync(new object[0]);
                Assert.NotNull(e);

                await target.CompleteAsync();
            }
        }

        [Fact]
        public async Task NoRows_Test()
        {
            using (var target = new AsyncDynamicObjectTarget())
            {
                await target.InitAsync(new ColumnInfo[]
                {
                    new ColumnInfo(name: "A", typeof(string))
                });

                await target.CompleteAsync();
            }
        }

        [Fact]
        public async Task WithRowsAndColumns_Test()
        {
            using (var target = new AsyncDynamicObjectTarget())
            {
                await target.InitAsync(new ColumnInfo[]
                {
                    new ColumnInfo(name: "A", typeof(string)),
                    new ColumnInfo(name: "B", typeof(int)),
                    new ColumnInfo(name: "SomeOtherProperty", typeof(int?))
                });

                {
                    var e = await target.WriteRowAsync(new object[] { "Hello", 42, 82 });
                    Assert.Equal("Hello", e.A);
                    Assert.Equal(42, e.B);
                    Assert.Equal(82, e.SomeOtherProperty);
                }

                {
                    var e = await target.WriteRowAsync(new object?[] { null, 42, null });
                    Assert.Equal(null, e.A);
                    Assert.Equal(42, e.B);
                    Assert.Equal(null, e.SomeOtherProperty);
                }
            }
        }
    }
}