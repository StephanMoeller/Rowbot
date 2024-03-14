using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Rowbot.Execution;

namespace Rowbot.Test.Execution
{
    public class RowbotExecutorBuilder_Test
    {
        [Fact]
        public void ToObjects_Test()
        {
            var input = Enumerable.Range(0, 3).Select(num => new UnitTestDto() {Name = num.ToString()});

            var result = new List<UnitTestDto>();
            new RowbotExecutorBuilder()
                .FromObjects(input)
                .ToObjects<UnitTestDto>(items => { result = items.ToList(); }).Execute();
            Assert.Equal(3, result.Count);
            Assert.Equal("0", result[0].Name);
            Assert.Equal("1", result[1].Name);
            Assert.Equal("2", result[2].Name);
        }

        private class UnitTestDto
        {
            public string Name { get; set; }
        }
    }
}