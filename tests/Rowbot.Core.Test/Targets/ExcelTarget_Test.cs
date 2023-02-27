using Rowbot.Core.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Core.Test.Targets
{
    public class ExcelTarget_Test
    {
        [Theory]
        [InlineData("A", 1)]
        [InlineData("B", 2)]
        [InlineData("C", 3)]
        [InlineData("D", 4)]
        [InlineData("E", 5)]
        [InlineData("F", 6)]
        [InlineData("G", 7)]

        [InlineData("H", 8)]
        [InlineData("I", 9)]
        [InlineData("J", 10)]
        [InlineData("K", 11)]
        [InlineData("L", 12)]
        [InlineData("M", 13)]
        [InlineData("N", 14)]

        [InlineData("O", 15)]
        [InlineData("P", 16)]
        [InlineData("Q", 17)]
        [InlineData("R", 18)]
        [InlineData("S", 19)]
        [InlineData("T", 20)]
        [InlineData("U", 21)]

        [InlineData("V", 22)]
        [InlineData("W", 23)]
        [InlineData("X", 24)]
        [InlineData("Y", 25)]
        [InlineData("Z", 26)]
        [InlineData("AA",27)]
        [InlineData("AB", 28)]
        [InlineData("AC", 29)]
        [InlineData("AY", 26 + 26 - 1)]
        [InlineData("AZ", 26 + 26)]
        [InlineData("BA", 26 + 26 + 1)]
        [InlineData("BB", 26 + 26 + 2)]
        [InlineData("BC", 26 + 26 + 3)]
        public void Foo(string expectedName, int oneBasedIndex)
        {
            var name = ExcelTarget.GetColumnName(oneBasedIndex);
            Assert.Equal(expectedName, name);
        }
    }
}
