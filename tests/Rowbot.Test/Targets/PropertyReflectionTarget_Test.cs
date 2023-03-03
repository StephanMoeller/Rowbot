using Rowbot.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Test.Targets
{
    public class PropertyReflectionTarget_Test
    {
        [Fact]
        public void Constructor_NoSetablePropertiesOnGenericType_ExpectException_Test()
        {
            Assert.Throws<ArgumentException>(() => new PropertyReflectionTarget<UnitTestDummy_NoSetters>());
        }

        [Fact]
        public void NoMatchingPropertiesWithColumnNames_Test()
        {
            var target = new PropertyReflectionTarget<UnitTestDummy>();

            target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "A", typeof(string)),
                    new ColumnInfo(name: "B", typeof(int))
                });

            {
                var e = target.WriteRow(new object[] { "Hello", 42 });
                Assert.Null(e.String_GetOnly);
                Assert.Null(e.Get_String_SetOnly_Value());
                Assert.Null(e.String_GetAndSet);
                Assert.Equal(0, e.Int_GetAndSet);
                Assert.Equal(0, e.AnotherInt_GetAndSet);
                Assert.Null(e.NullableInt_GetAndSet);
            }

            {
                var e = target.WriteRow(new object[] { "Hello again", 82 });
                Assert.Null(e.String_GetOnly);
                Assert.Null(e.Get_String_SetOnly_Value());
                Assert.Null(e.String_GetAndSet);
                Assert.Equal(0, e.Int_GetAndSet);
                Assert.Equal(0, e.AnotherInt_GetAndSet);
                Assert.Null(e.NullableInt_GetAndSet);
            }

            target.Complete();
        }

        [Fact]
        public void WithMatchingProperties_Test()
        {
            var target = new PropertyReflectionTarget<UnitTestDummy>();

            target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "string_SETONLY", typeof(string)),
                    new ColumnInfo(name: "and_this_one_does_not_match_anything", typeof(string)),
                    new ColumnInfo(name: "int_getandset", typeof(int))
                });

            {
                var e = target.WriteRow(new object[] { "Hello", "ignored", 42 });
                Assert.Null(e.String_GetOnly);
                Assert.Equal("Hello", e.Get_String_SetOnly_Value());
                Assert.Null(e.String_GetAndSet);
                Assert.Equal(42, e.Int_GetAndSet);
                Assert.Equal(0, e.AnotherInt_GetAndSet);
                Assert.Null(e.NullableInt_GetAndSet);
            }

            {
                var e = target.WriteRow(new object[] { "Hello again", "ignored", 82 });
                Assert.Null(e.String_GetOnly);
                Assert.Equal("Hello again", e.Get_String_SetOnly_Value());
                Assert.Null(e.String_GetAndSet);
                Assert.Equal(82, e.Int_GetAndSet);
                Assert.Equal(0, e.AnotherInt_GetAndSet);
                Assert.Null(e.NullableInt_GetAndSet);
            }

            target.Complete();
        }

        [Fact]
        public void NullableIntOnProperty_NotNullableIntInSource_EnsureWorks_Test()
        {
            var target = new PropertyReflectionTarget<UnitTestDummy>();

            target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "NullableInt_GetAndSet", typeof(int))
                });

            var dummy = target.WriteRow(new object[] { 42 });

            target.Complete();

            Assert.Equal(42, dummy.NullableInt_GetAndSet);

        }

        [Theory]
        [InlineData(42)]
        [InlineData(null)]
        public void NullableIntOnProperty_NullableIntInSource_EnsureWorks_Test(int? inputValue)
        {
            var target = new PropertyReflectionTarget<UnitTestDummy>();

            target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "NullableInt_GetAndSet", typeof(int?))
                });

            var dummy = target.WriteRow(new object?[] { inputValue });

            target.Complete();

            Assert.Equal(inputValue, dummy.NullableInt_GetAndSet);
        }

        [Theory]
        [InlineData(42, true)]
        [InlineData(null, false)]
        public void IntOnProperty_NullableIntInSource_EnsureValuesAreCopiedWhenNotNullInSource_Test(int? inputValue, bool expectSuccess)
        {
            var target = new PropertyReflectionTarget<UnitTestDummy>();

            target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "Int_GetAndSet", typeof(int?))
                });

            if (expectSuccess)
            {
                var dummy = target.WriteRow(new object?[] { inputValue });

                target.Complete();

                Assert.Equal(inputValue, dummy.Int_GetAndSet);
            }
            else
            {
                // Expect exception when setting to null
                Assert.Throws<ArgumentNullException>(() =>
                {
                    target.WriteRow(new object?[] { inputValue });
                });
            }
        }

        [Fact]
        public void TypeMismatch_Test()
        {
            var target = new PropertyReflectionTarget<UnitTestDummy>();
            Assert.Throws<TypeMismatchException>(() => target.Init(new ColumnInfo[] { new ColumnInfo(name: "string_SETONLY", typeof(int)) }));
        }
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class UnitTestDummy_NoSetters
    {
        public string GetOnly { get; }
    }

    public class UnitTestDummy
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
