using DocumentFormat.OpenXml.Bibliography;
using Rowbot.Sources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Test.Sources
{
    public class DynamicObjectSource_Test
    {
        [Fact]
        public void GetColumnsAndRows_NoRows_Test()
        {
            List<dynamic> emptyList = new List<dynamic>();

            var source = new DynamicObjectSource(emptyList);
            var columns = source.InitAndGetColumns().ToArray();
            Assert.Empty(columns);
            
            var rows = TestUtils.ReadAllLines(source, columnCount: columns.Length);
            Assert.Empty(rows);
        }
        
        [Fact]
        public void GetColumnsAndRows_NoColumns_ExpectException_Test()
        {
            var dynamicObjects = Enumerable.Range(1, 10).Select(x =>
            {
                dynamic runTimeObject = new ExpandoObject();
                return runTimeObject;
            }).ToArray();
            Assert.Equal(10, dynamicObjects.Length);

            var anonymouseObjects = Enumerable.Range(0, 3).Select(e => new { });
            var source = new DynamicObjectSource(dynamicObjects);

            var columns = source.InitAndGetColumns().ToArray();
            Assert.Empty(columns);

            var rows = TestUtils.ReadAllLines(source, columnCount: columns.Length);
            Assert.Equal(10, rows.Length);
        }

        [Fact]
        public void GetColumnsAndRows_Test()
        {
            var dynamicObjects = Enumerable.Range(0, 3).Select(x =>
            {
                dynamic runTimeObject = new ExpandoObject();
                runTimeObject.Value = x;
                runTimeObject.Name = "Hello number " + x;
                runTimeObject.NullValue = null;
                return runTimeObject;
            }).ToArray();

            var source = new DynamicObjectSource(dynamicObjects);

            var columns = source.InitAndGetColumns().ToArray();
            Assert.Equal(3, columns.Length);

            Assert.Equal("Value", columns[0].Name);
            Assert.Equal(typeof(object), columns[0].ValueType);

            Assert.Equal("Name", columns[1].Name);
            Assert.Equal(typeof(object), columns[1].ValueType);

            Assert.Equal("NullValue", columns[2].Name);
            Assert.Equal(typeof(object), columns[2].ValueType);

            // Assert row elements
            var rows = TestUtils.ReadAllLines(source, columnCount: columns.Length);

            Assert.Equal(3, rows.Length);
            AssertArraysEqual(new object?[] { 0, "Hello number 0", null }, rows[0]);
            AssertArraysEqual(new object?[] { 1, "Hello number 1", null }, rows[1]);
            AssertArraysEqual(new object?[] { 2, "Hello number 2", null }, rows[2]);

        }


        [Fact]
        public void GetColumnsAndRows_DynamicObjectsHaveDifferentProperties_ExpectException_Test()
        {
            var differentObjects = new List<dynamic>();

            {
                dynamic obj1 = new ExpandoObject();
                obj1.PropA = 10;
                obj1.PropB = "Hello there";
                differentObjects.Add(obj1);
            }

            {
                dynamic obj2 = new ExpandoObject();
                obj2.PropB = 9876;
                obj2.PropC = "Another field";
                differentObjects.Add(obj2);
            }

            var source = new DynamicObjectSource(differentObjects);

            var columns = source.InitAndGetColumns().ToArray();
            Assert.Equal(2, columns.Length);

            Assert.Equal("PropA", columns[0].Name);
            Assert.Equal(typeof(object), columns[0].ValueType);

            Assert.Equal("PropB", columns[1].Name);
            Assert.Equal(typeof(object), columns[1].ValueType);

            // 
            var ex = Assert.Throws<DynamicObjectsNotIdenticalException>(() => TestUtils.ReadAllLines(source, columnCount: columns.Length));
            Assert.Equal("Two dynamic objects was not identical in collection. One had keys: [PropA,PropB] and another one had keys: [PropB, PropC]", ex.Message);
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
