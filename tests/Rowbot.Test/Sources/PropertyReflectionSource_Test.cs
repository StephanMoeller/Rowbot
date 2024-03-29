﻿using Rowbot.Sources;
using System.Data;
using System.Dynamic;

namespace Rowbot.Test.Sources
{
    public class PropertyReflectionSource_Test
    {
        [Fact]
        public void TestWithDynamicObjects()
        {
            var dynamicObjects = Enumerable.Range(1, 10).Select(x =>
            {
                dynamic runTimeObject = new ExpandoObject();
                runTimeObject.Name = "OnTheFlyFooObject";
                runTimeObject.Value = 123;
                return runTimeObject;
            }).ToArray();

            Assert.Throws< ArgumentException>(() => PropertyReflectionSource.Create(elements: dynamicObjects));
        }

        [Fact]
        public void GetColumnsAndRows_NoRows_Test()
        {
            var anonymouseObjects = Enumerable.Range(0, 0).Select(e => new
            {
                myInt = 123,
                myString = "Helloooo δ!2",
                myDecimal = -12.34m,
                myNullableDecimal = (decimal?)null,
                æøÅÆØÅSpecial = "ok"
            });

            var source = PropertyReflectionSource.Create(elements: anonymouseObjects);
            var columns = source.InitAndGetColumns().ToArray();
            Assert.Equal(5, columns.Length);
            Assert.Equal("myInt", columns[0].Name);
            Assert.Equal(typeof(int), columns[0].ValueType);

            Assert.Equal("myString", columns[1].Name);
            Assert.Equal(typeof(string), columns[1].ValueType);

            Assert.Equal("myDecimal", columns[2].Name);
            Assert.Equal(typeof(decimal), columns[2].ValueType);

            Assert.Equal("myNullableDecimal", columns[3].Name);
            Assert.Equal(typeof(decimal?), columns[3].ValueType);

            Assert.Equal("æøÅÆØÅSpecial", columns[4].Name);
            Assert.Equal(typeof(string), columns[4].ValueType);

            var rows = TestUtils.ReadAllLines(source, columnCount: columns.Length);
            Assert.Empty(rows);
        }

        [Fact]
        public void GetColumnsAndRows_NoColumns_Test()
        {
            var anonymouseObjects = Enumerable.Range(0, 3).Select(e => new { });
            var reader = PropertyReflectionSource.Create(elements: anonymouseObjects);
            Assert.Empty(reader.InitAndGetColumns());
        }

        [Fact]
        public void GetColumnsAndRows_Test()
        {
            var anonymouseObjects = Enumerable.Range(0, 3).Select(e => new
            {
                myInt = e,
                myString = "Hello number " + e,
                myDecimal = e + -12.34m,
                myNullableDecimal = (decimal?)null,
                æøÅÆØÅSpecial = new DateTime(2000 + e, 02, 03, 14, 05, 06)
            });

            var source = PropertyReflectionSource.Create(elements: anonymouseObjects);

            var columns = source.InitAndGetColumns().ToArray();
            Assert.Equal("myInt", columns[0].Name);
            Assert.Equal(typeof(int), columns[0].ValueType);

            Assert.Equal("myString", columns[1].Name);
            Assert.Equal(typeof(string), columns[1].ValueType);

            Assert.Equal("myDecimal", columns[2].Name);
            Assert.Equal(typeof(decimal), columns[2].ValueType);

            Assert.Equal("myNullableDecimal", columns[3].Name);
            Assert.Equal(typeof(decimal?), columns[3].ValueType);

            Assert.Equal("æøÅÆØÅSpecial", columns[4].Name);
            Assert.Equal(typeof(DateTime), columns[4].ValueType);

            // Assert row elements
            var rows = TestUtils.ReadAllLines(source, columnCount: columns.Length);

            Assert.Equal(3, rows.Length);
            AssertArraysEqual(new object?[] { 0, "Hello number 0", -12.34m, null, new DateTime(2000, 02, 03, 14, 05, 06) }, rows[0]);
            AssertArraysEqual(new object?[] { 1, "Hello number 1", -11.34m, null, new DateTime(2001, 02, 03, 14, 05, 06) }, rows[1]);
            AssertArraysEqual(new object?[] { 2, "Hello number 2", -10.34m, null, new DateTime(2002, 02, 03, 14, 05, 06) }, rows[2]);
        }

        private static void AssertArraysEqual(object?[] val1, object[] val2)
        {
            Assert.Equal(val1.Length, val2.Length);
            for (var i = 0; i < val1.Length; i++)
            {
                if (val1[i] == null && val2[i] == null)
                    return;

                if (!Equals(val1[i], val2[i]))
                {
                    Assert.True(false, $"Value at position {i} has value {val1[i]} ({val1[i]?.GetType().Name ?? "NULL"}) in val1, but value {val2[i]} ({val2[i]?.GetType().Name ?? "NULL"}) in val2");
                }
            }
        }

    }
}
