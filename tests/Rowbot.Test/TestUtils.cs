namespace Rowbot.Test
{
    internal static class TestUtils
    {
        public static object[][] ReadAllLines(IRowSource source, int columnCount)
        {
            var allLines = new List<object[]>();
            var buffer = new object[columnCount];
            while (source.ReadRow(buffer))
            {
                allLines.Add(buffer.ToArray());
            }
            return allLines.ToArray();
        }
        
        public static async Task<object[][]> ReadAllLinesAsync(IAsyncRowSource source, int columnCount)
        {
            var allLines = new List<object[]>();
            var buffer = new object[columnCount];
            while (await source.ReadRowAsync(buffer))
            {
                allLines.Add(buffer.ToArray());
            }
            return allLines.ToArray();
        }
    }
}
