using System;
using System.Collections.Generic;
using System.Text;

namespace Rowbot
{
    public class ColumnInfo
    {
        public string Name{ get; }
        public Type ValueType { get; }

        public ColumnInfo(string name, Type valueType)
        {
            Name = name;
            ValueType = valueType;
        }
    }
}
