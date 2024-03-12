using Rowbot.Sources;
using System.Dynamic;

namespace Rowbot.Test.Sources
{
    public class AsyncDynamicObjectSource_Test
    {
        [Fact]
        public async Task GetColumnsAndRows_NoRows_Test()
        {
            var emptyList = AsyncEnumerable.Empty<dynamic>();

            var source = new AsyncDynamicObjectSource(emptyList);
            var columns = (await source.InitAndGetColumnsAsync()).ToArray();
            Assert.Empty(columns);
            
            var rows = await TestUtils.ReadAllLinesAsync(source, columnCount: columns.Length);
            Assert.Empty(rows);
        }
        
        [Fact]
        public async Task GetColumnsAndRows_NoColumns_ExpectException_Test()
        {
            var dynamicObjects = AsyncEnumerable.Range(1, 10).Select(x =>
            {
                dynamic runTimeObject = new ExpandoObject();
                return runTimeObject;
            });
            
            var source = new AsyncDynamicObjectSource(dynamicObjects);

            var columns = await source.InitAndGetColumnsAsync();
            Assert.Empty(columns);

            var rows = await TestUtils.ReadAllLinesAsync(source, columnCount: columns.Length);
            Assert.Equal(10, rows.Length);
        }

        [Fact]
        public async Task GetColumnsAndRows_Test()
        {
            var dynamicObjects = AsyncEnumerable.Range(0, 3).Select(x =>
            {
                dynamic runTimeObject = new ExpandoObject();
                runTimeObject.Value = x;
                runTimeObject.Name = "Hello number " + x;
                runTimeObject.NullValue = null;
                return runTimeObject;
            });

            var source = new AsyncDynamicObjectSource(dynamicObjects);

            var columns = await source.InitAndGetColumnsAsync();
            Assert.Equal(3, columns.Length);

            Assert.Equal("Value", columns[0].Name);
            Assert.Equal(typeof(object), columns[0].ValueType);

            Assert.Equal("Name", columns[1].Name);
            Assert.Equal(typeof(object), columns[1].ValueType);

            Assert.Equal("NullValue", columns[2].Name);
            Assert.Equal(typeof(object), columns[2].ValueType);

            // Assert row elements
            var rows = await TestUtils.ReadAllLinesAsync(source, columnCount: columns.Length);

            Assert.Equal(3, rows.Length);
            AssertArraysEqual(new object?[] { 0, "Hello number 0", null }, rows[0]);
            AssertArraysEqual(new object?[] { 1, "Hello number 1", null }, rows[1]);
            AssertArraysEqual(new object?[] { 2, "Hello number 2", null }, rows[2]);
        }

        [Fact]
        public async Task GetColumnsAndRows_DynamicObjectsHaveDifferentProperties_ExpectException_Test()
        {
            dynamic obj1 = new ExpandoObject();
            obj1.PropA = 10;
            obj1.PropB = "Hello there";
                
            dynamic obj2 = new ExpandoObject();
            obj2.PropB = 9876;
            obj2.PropC = "Another field";
            
            var objects = new [] { obj1, obj2 };
            var differentObjects = AsyncEnumerable.Range(0, 2).Select(index => objects[index]);

            var source = new AsyncDynamicObjectSource(differentObjects);

            var columns = await source.InitAndGetColumnsAsync();
            Assert.Equal(2, columns.Length);

            Assert.Equal("PropA", columns[0].Name);
            Assert.Equal(typeof(object), columns[0].ValueType);

            Assert.Equal("PropB", columns[1].Name);
            Assert.Equal(typeof(object), columns[1].ValueType);
            
            var ex = await Assert.ThrowsAsync<DynamicObjectsNotIdenticalException>(() => TestUtils.ReadAllLinesAsync(source, columnCount: columns.Length));
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
