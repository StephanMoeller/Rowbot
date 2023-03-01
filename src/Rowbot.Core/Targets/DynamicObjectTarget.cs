using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Rowbot.Core.Targets
{
    public class DynamicObjectTarget : IRowTarget
    {
        private string[] _columnNames;
        private LinkedList<dynamic> _dynamicList = new LinkedList<dynamic>();
        public void Dispose()
        {
            
        }

        public void Complete()
        {
            
        }

        public LinkedList<dynamic> GetResult()
        {
            return _dynamicList;
        }

        public void Init(ColumnInfo[] columns)
        {
            _columnNames = columns.Select(c => c.Name).ToArray();
        }

        public void WriteRow(object[] values)
        {
            IDictionary<string, object> newObject = new ExpandoObject();
            for (var i = 0; i < values.Length; i++)
            {
                newObject.Add(_columnNames[i], values[i]);
            }
            _dynamicList.AddLast(newObject);
        }
    }
}
