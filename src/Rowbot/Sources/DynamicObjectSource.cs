using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rowbot.Sources
{
    public class DynamicObjectSource : IRowSource, IDisposable
    {
        private readonly IEnumerator<dynamic> _enumerator;
        private string[] _keys;
        private bool _hasCurrentItem = true;
        public DynamicObjectSource(IEnumerable<dynamic> objects)
        {
            if (objects is null)
            {
                throw new ArgumentNullException(nameof(objects));
            }

            this._enumerator = objects.GetEnumerator();
        }

        public ColumnInfo[] InitAndGetColumns()
        {
            _hasCurrentItem = _enumerator.MoveNext();
            if (!_hasCurrentItem)
            {
                // There are no rows at all - return an empty column list as there are no dynamic objects available to reveal the properties
                _keys = new string[0];
                return new ColumnInfo[0];
            }

            var current = (IDictionary<string, object>)_enumerator.Current;
            _keys = current.Keys.ToArray();
            return _keys.Select(k => new ColumnInfo(name: k, valueType: typeof(object))).ToArray();
        }

        public bool ReadRow(object[] values)
        {
            if (!_hasCurrentItem)
                return false;

            var current = (IDictionary<string, object>)_enumerator.Current;

            AssertEqualsExpectedKeysOrThrow(current.Keys);

            for (var i = 0; i < _keys.Length; i++)
            {
                values[i] = current[_keys[i]];
            }

            _hasCurrentItem = _enumerator.MoveNext();
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

        public void Complete()
        {
            // Nothing to complete
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }

    public class DynamicObjectsNotIdenticalException : Exception
    {
        public DynamicObjectsNotIdenticalException(string message) : base(message) { }
    }
}
