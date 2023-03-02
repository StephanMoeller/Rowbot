using System;
using System.Collections.Generic;
using System.Text;

namespace Rowbot
{
    public interface IRowTarget
    {
        void Init(ColumnInfo[] columns);
        void WriteRow(object[] values);
        void Complete();
    }
}
