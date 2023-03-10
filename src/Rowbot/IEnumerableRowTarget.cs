using System;

namespace Rowbot
{
    public interface IEnumerableRowTarget<TOutputType>
    {
        void Init(ColumnInfo[] columns);
        TOutputType WriteRow(object[] values);
        void Complete();
    }
}
