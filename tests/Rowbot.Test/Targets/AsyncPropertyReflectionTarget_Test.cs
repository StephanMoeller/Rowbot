using Rowbot.Targets;

namespace Rowbot.Test.Targets
{
    public class AsyncPropertyReflectionTarget_Test
    {
        [Fact]
        public void Constructor_NoSettablePropertiesOnGenericType_ExpectException_Test()
        {
            Assert.Throws<ArgumentException>(() => new AsyncPropertyReflectionTarget<UnitTestDummy_NoSetters>());
        }

        [Fact]
        public async Task NoMatchingPropertiesWithColumnNames_Test()
        {
            var target = new AsyncPropertyReflectionTarget<UnitTestDummy>();

            await target.InitAsync(new ColumnInfo[]
            {
                new ColumnInfo(name: "A", typeof(string)),
                new ColumnInfo(name: "B", typeof(int))
            });

            {
                var e = await target.WriteRowAsync(new object[] { "Hello", 42 });
                Assert.Null(e.String_GetOnly);
                Assert.Null(e.Get_String_SetOnly_Value());
                Assert.Null(e.String_GetAndSet);
                Assert.Equal(0, e.Int_GetAndSet);
                Assert.Equal(0, e.AnotherInt_GetAndSet);
                Assert.Null(e.NullableInt_GetAndSet);
            }

            {
                var e = await target.WriteRowAsync(new object[] { "Hello again", 82 });
                Assert.Null(e.String_GetOnly);
                Assert.Null(e.Get_String_SetOnly_Value());
                Assert.Null(e.String_GetAndSet);
                Assert.Equal(0, e.Int_GetAndSet);
                Assert.Equal(0, e.AnotherInt_GetAndSet);
                Assert.Null(e.NullableInt_GetAndSet);
            }

            await target.CompleteAsync();
        }

        [Fact]
        public async Task WithMatchingProperties_Test()
        {
            var target = new AsyncPropertyReflectionTarget<UnitTestDummy>();

            await target.InitAsync(new ColumnInfo[]
            {
                new ColumnInfo(name: "string_SETONLY", typeof(string)),
                new ColumnInfo(name: "and_this_one_does_not_match_anything", typeof(string)),
                new ColumnInfo(name: "int_getandset", typeof(int))
            });

            {
                var e = await target.WriteRowAsync(new object[] { "Hello", "ignored", 42 });
                Assert.Null(e.String_GetOnly);
                Assert.Equal("Hello", e.Get_String_SetOnly_Value());
                Assert.Null(e.String_GetAndSet);
                Assert.Equal(42, e.Int_GetAndSet);
                Assert.Equal(0, e.AnotherInt_GetAndSet);
                Assert.Null(e.NullableInt_GetAndSet);
            }

            {
                var e = await target.WriteRowAsync(new object[] { "Hello again", "ignored", 82 });
                Assert.Null(e.String_GetOnly);
                Assert.Equal("Hello again", e.Get_String_SetOnly_Value());
                Assert.Null(e.String_GetAndSet);
                Assert.Equal(82, e.Int_GetAndSet);
                Assert.Equal(0, e.AnotherInt_GetAndSet);
                Assert.Null(e.NullableInt_GetAndSet);
            }

            await target.CompleteAsync();
        }

        [Fact]
        public async Task NullableIntOnProperty_NotNullableIntInSource_EnsureWorks_Test()
        {
            var target = new AsyncPropertyReflectionTarget<UnitTestDummy>();

            await target.InitAsync(new ColumnInfo[]
            {
                new ColumnInfo(name: "NullableInt_GetAndSet", typeof(int))
            });

            var dummy = await target.WriteRowAsync(new object[] { 42 });

            await target.CompleteAsync();

            Assert.Equal(42, dummy.NullableInt_GetAndSet);
        }

        [Theory]
        [InlineData(42)]
        [InlineData(null)]
        public async Task NullableIntOnProperty_NullableIntInSource_EnsureWorks_Test(int? inputValue)
        {
            var target = new AsyncPropertyReflectionTarget<UnitTestDummy>();

            await target.InitAsync(new ColumnInfo[]
            {
                new ColumnInfo(name: "NullableInt_GetAndSet", typeof(int?))
            });

            var dummy = await target.WriteRowAsync(new object?[] { inputValue });

            await target.CompleteAsync();

            Assert.Equal(inputValue, dummy.NullableInt_GetAndSet);
        }

        [Theory]
        [InlineData(42, true)]
        [InlineData(null, false)]
        public async Task IntOnProperty_NullableIntInSource_EnsureValuesAreCopiedWhenNotNullInSource_Test(int? inputValue,
            bool expectSuccess)
        {
            var target = new AsyncPropertyReflectionTarget<UnitTestDummy>();

            await target.InitAsync(new ColumnInfo[]
            {
                new ColumnInfo(name: "Int_GetAndSet", typeof(int?))
            });

            if (expectSuccess)
            {
                var dummy = await target.WriteRowAsync(new object?[] { inputValue });

                await target.CompleteAsync();

                Assert.Equal(inputValue, dummy.Int_GetAndSet);
            }
            else
            {
                // Expect exception when setting to null
                _ = await Assert.ThrowsAsync<ArgumentNullException>(() => target.WriteRowAsync(new object?[] { inputValue }));
            }
        }

        [Fact]
        public async Task TypeMismatch_Test()
        {
            var target = new AsyncPropertyReflectionTarget<UnitTestDummy>();
            _ = await Assert.ThrowsAsync<TypeMismatchException>(() =>
                target.InitAsync(new ColumnInfo[] { new ColumnInfo(name: "string_SETONLY", typeof(int)) }));
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private class UnitTestDummy_NoSetters
        {
            public string GetOnly { get; }
        }

        private class UnitTestDummy
        {
            public string String_GetOnly { get; private set; }
            public string String_SetOnly { private get; set; }
            public string String_GetAndSet { get; set; }
            public int Int_GetAndSet { get; set; }
            public int AnotherInt_GetAndSet { get; set; }
            public int? NullableInt_GetAndSet { get; set; }

            public string Get_String_SetOnly_Value()
            {
                return String_SetOnly;
            }
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}