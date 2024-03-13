using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rowbot.Targets
{
    public class AsyncDynamicObjectTarget : IDisposable // Not possible: : IEnumerableRowTarget<dynamic>
    {
        private string[] _columnNames;

        public AsyncDynamicObjectTarget()
        {
        }

        public void Dispose()
        {
        }

        public Task CompleteAsync()
        {
            return Task.CompletedTask;
        }

        public Task InitAsync(ColumnInfo[] columns)
        {
            _columnNames = columns.Select(c => c.Name).ToArray();
            
            return Task.CompletedTask;
        }

        public Task<dynamic> WriteRowAsync(object[] values)
        {
            IDictionary<string, object> newObject = new ExpandoObject();
            for (var i = 0; i < values.Length; i++)
            {
                newObject.Add(_columnNames[i], values[i]);
            }
            return Task.FromResult<dynamic>(newObject);
        }
    }
}
