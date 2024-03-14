using Rowbot.Execution;

namespace Rowbot.Test.Execution
{
    public class RowbotAsyncExecutorBuilder_Test
    {
        [Fact]
        public async Task ToObjects_Test()
        {
            var input = AsyncEnumerable.Range(0, 3).Select(num => new UnitTestDto() {Name = num.ToString()});

            var result = new List<UnitTestDto>();
            await new RowbotAsyncExecutorBuilder()
                .FromObjects<UnitTestDto>(input)
                .ToObjects<UnitTestDto>(async items =>
                {
                    await foreach (var i in items)
                    {
                        result.Add(i);
                    }
                }).ExecuteAsync();
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