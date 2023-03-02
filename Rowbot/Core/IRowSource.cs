using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rowbot
{
    public interface IRowSource
    {
        ColumnInfo[] InitAndGetColumns();
        bool ReadRow(object[] values);
        void Complete();
    }

}
