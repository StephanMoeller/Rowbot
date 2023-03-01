using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Test
{
    internal class TestUtils
    {
        public static object[][] ReadAllLines(IRowSource source, int columnCount)
        {
            var allLines = new List<object[]>();
            var buffer = new object[columnCount];
            while(source.ReadRow(buffer)){
                allLines.Add(buffer.ToArray());
            }
            return allLines.ToArray();
        }
    }
}
