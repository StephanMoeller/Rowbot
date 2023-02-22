using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Rowbot.Core.Targets
{
    public class DynamicObjectTarget : RowTarget
    {
        private string[] _columnNames;
        private LinkedList<dynamic> _dynamicList = new LinkedList<dynamic>();
        public override void Dispose()
        {
            
        }

        protected override void OnComplete()
        {
            
        }

        public LinkedList<dynamic> GetResult()
        {
            return _dynamicList;
        }

        protected override void OnInit(ColumnInfo[] columns)
        {
            _columnNames = columns.Select(c => c.Name).ToArray();
        }

        protected override void OnWriteRow(object[] values)
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
