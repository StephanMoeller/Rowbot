using System;
using System.Collections.Generic;
using System.Text;

namespace Rowbot.Core.Sources
{
    public class DynamicObjectSource : RowSource
    {
        public DynamicObjectSource(IEnumerable<dynamic> objects)
        {

        }
        public override void Dispose()
        {
            
        }

        protected override void OnComplete()
        {
            
        }

        protected override ColumnInfo[] OnInitAndGetColumns()
        {
            
        }

        protected override bool OnReadRow(object[] values)
        {
            
        }
    }
}
