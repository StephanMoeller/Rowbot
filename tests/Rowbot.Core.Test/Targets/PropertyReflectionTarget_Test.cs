﻿using Rowbot.Core.Targets;
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
            using (var target = new PropertyReflectionTarget<UnitTestDummy>())
            {
                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "A", typeof(string)),
                    new ColumnInfo(name: "B", typeof(int))
                });

                target.WriteRow(new object[] { "Hello", 42 });
                target.WriteRow(new object[] { "Hello again", 82 });

                target.Complete();

                var result = target.GetResult().ToArray();

                Assert.Equal(2, result.Length);

                {
                    var e = result[0];
                    Assert.Null(e.String_GetOnly);
                    Assert.Null(e.Get_String_SetOnly_Value());
                    Assert.Null(e.String_GetAndSet);
                    Assert.Equal(0, e.Int_GetAndSet);
                    Assert.Equal(0, e.AnotherInt_GetAndSet);
                    Assert.Null(e.NullableInt_GetAndSet);
                }

                {
                    var e = result[1];
                    Assert.Null(e.String_GetOnly);
                    Assert.Null(e.Get_String_SetOnly_Value());
                    Assert.Null(e.String_GetAndSet);
                    Assert.Equal(0, e.Int_GetAndSet);
                    Assert.Equal(0, e.AnotherInt_GetAndSet);
                    Assert.Null(e.NullableInt_GetAndSet);
                }
            }
        }

        [Fact]
        public void WithMatchingProperties_Test()
        {
            using (var target = new PropertyReflectionTarget<UnitTestDummy>())
            {
                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "string_SETONLY", typeof(string)),
                    new ColumnInfo(name: "and_this_one_does_not_match_anything", typeof(string)),
                    new ColumnInfo(name: "int_getandset", typeof(int))
                });

                target.WriteRow(new object[] { "Hello", "ignored", 42 });
                target.WriteRow(new object[] { "Hello again", "ignored", 82 });

                target.Complete();

                var result = target.GetResult().ToArray();

                Assert.Equal(2, result.Length);

                {
                    var e = result[0];
                    Assert.Null(e.String_GetOnly);
                    Assert.Equal("Hello", e.Get_String_SetOnly_Value());
                    Assert.Null(e.String_GetAndSet);
                    Assert.Equal(42, e.Int_GetAndSet);
                    Assert.Equal(0, e.AnotherInt_GetAndSet);
                    Assert.Null(e.NullableInt_GetAndSet);
                }

                {
                    var e = result[1];
                    Assert.Null(e.String_GetOnly);
                    Assert.Equal("Hello again", e.Get_String_SetOnly_Value());
                    Assert.Null(e.String_GetAndSet);
                    Assert.Equal(82, e.Int_GetAndSet);
                    Assert.Equal(0, e.AnotherInt_GetAndSet);
                    Assert.Null(e.NullableInt_GetAndSet);
                }
            }
        }

        [Fact]
        public void NullableIntOnProperty_NotNullableIntInSource_EnsureWorks_Test()
        {
            using (var target = new PropertyReflectionTarget<UnitTestDummy>())
            {
                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "NullableInt_GetAndSet", typeof(int))
                });

                target.WriteRow(new object[] { 42 });

                target.Complete();

                var result = target.GetResult().ToArray();

                Assert.Single(result);
                Assert.Equal(42, result.Single().NullableInt_GetAndSet);
            }
        }

        [Theory]
        [InlineData(42)]
        [InlineData(null)]
        public void NullableIntOnProperty_NullableIntInSource_EnsureWorks_Test(int? inputValue)
        {
            using (var target = new PropertyReflectionTarget<UnitTestDummy>())
            {
                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "NullableInt_GetAndSet", typeof(int?))
                });

                target.WriteRow(new object?[] { inputValue });

                target.Complete();

                var result = target.GetResult().ToArray();

                Assert.Single(result);
                Assert.Equal(inputValue, result.Single().NullableInt_GetAndSet);
            }
        }

        [Theory]
        [InlineData(42, true)]
        [InlineData(null, false)]
        public void IntOnProperty_NullableIntInSource_EnsureValuesAreCopiedWhenNotNullInSource_Test(int? inputValue, bool expectSuccess)
        {
            using (var target = new PropertyReflectionTarget<UnitTestDummy>())
            {
                target.Init(new ColumnInfo[]{
                    new ColumnInfo(name: "Int_GetAndSet", typeof(int?))
                });

                if (expectSuccess)
                {
                    target.WriteRow(new object?[] { inputValue });

                    target.Complete();

                    var result = target.GetResult().ToArray();

                    Assert.Single(result);
                    Assert.Equal(inputValue, result.Single().Int_GetAndSet);
                }
                else
                {
                    // Expect exception when setting to null
                    Assert.Throws<ArgumentNullException>(() => {
                        target.WriteRow(new object?[] { inputValue });
                    });
                }
            }
        }

        [Fact]
        public void TypeMismatch_Test()
        {
            using (var target = new PropertyReflectionTarget<UnitTestDummy>())
            {
                Assert.Throws<TypeMismatchException>(() => target.Init(new ColumnInfo[] { new ColumnInfo(name: "string_SETONLY", typeof(int)) }));
            }
        }
    }

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
}