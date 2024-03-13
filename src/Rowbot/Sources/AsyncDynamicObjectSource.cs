using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rowbot.Sources
{
    public class AsyncDynamicObjectSource : IAsyncRowSource, IDisposable
    {
        private readonly IAsyncEnumerator<dynamic> _enumerator;
        private string[] _keys;
        private bool _hasCurrentItem = true;
        public AsyncDynamicObjectSource(IAsyncEnumerable<dynamic> objects)
        {
            if (objects is null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            this._enumerator = objects.GetAsyncEnumerator();
        }

        public async Task<ColumnInfo[]> InitAndGetColumnsAsync()
        {
            _hasCurrentItem = await _enumerator.MoveNextAsync();
            if (!_hasCurrentItem)
            {
                // There are no rows at all - return an empty column list as there are no dynamic objects available to reveal the properties
                _keys = Array.Empty<string>();
                return Array.Empty<ColumnInfo>();
            }

            var current = (IDictionary<string, object>)_enumerator.Current;
            _keys = current.Keys.ToArray();
            return _keys.Select(k => new ColumnInfo(name: k, valueType: typeof(object))).ToArray();
        }

        public async Task<bool> ReadRowAsync(object[] values)
        {
            if (!_hasCurrentItem)
                return false;

            var current = (IDictionary<string, object>)_enumerator.Current;

            AssertEqualsExpectedKeysOrThrow(current.Keys);

            for (var i = 0; i < _keys.Length; i++)
            {
                values[i] = current[_keys[i]];
            }

            _hasCurrentItem = await _enumerator.MoveNextAsync();
            return true;
        }

        private void AssertEqualsExpectedKeysOrThrow(ICollection<string> keys)
        {
            if (keys.Count != _keys.Length)
                throw new DynamicObjectsNotIdenticalException($"Two dynamic objects was not identical in collection. One had keys: [{string.Join(",", _keys)}] and another one had keys: [{string.Join(", ", keys)}]");

            int i = 0;
            foreach(var key in keys)
            {
                if (key != _keys[i])
                {
                    throw new DynamicObjectsNotIdenticalException($"Two dynamic objects was not identical in collection. One had keys: [{string.Join(",", _keys)}] and another one had keys: [{string.Join(", ", keys)}]");
                }
                i++;
            }
        }

        public Task CompleteAsync()
        {
            // Nothing to complete
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
