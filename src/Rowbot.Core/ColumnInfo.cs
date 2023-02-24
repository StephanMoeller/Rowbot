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
            Name = name ?? throw new ArgumentNullException(paramName: nameof(name), message: "Name cannot be null. Specify an empty string if no column name can be determined from a source.");
            ValueType = valueType;
        }
    }
}
