using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Rowbot.Targets
{
    public class DynamicObjectTarget : IDisposable // Not possible: : IEnumerableRowTarget<dynamic>
    {
        private string[] _columnNames;

        public DynamicObjectTarget()
        {
        }

        public void Dispose()
        {

        }

        public void Complete()
        {

        }

        public void Init(ColumnInfo[] columns)
        {
            _columnNames = columns.Select(c => c.Name).ToArray();
        }

        public dynamic WriteRow(object[] values)
        {
            IDictionary<string, object> newObject = new ExpandoObject();
            for (var i = 0; i < values.Length; i++)
            {
                newObject.Add(_columnNames[i], values[i]);
            }
            return newObject;
        }
    }
}
