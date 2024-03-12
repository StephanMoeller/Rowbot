using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rowbot.Sources
{
    public class AsyncPropertyReflectionSource<T> : IAsyncRowSource
    {
        private readonly IAsyncEnumerator<T> _elements;
        private readonly PropertyInfo[] _properties;
        private readonly ColumnInfo[] _columns;
        private AsyncPropertyReflectionSource(IAsyncEnumerator<T> elements, Type elementType)
        {
            if (elements is null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            if (elementType is null)
            {
                throw new ArgumentNullException(nameof(elementType));
            }

            _elements = elements;
            _properties = elementType.GetProperties();
            _columns = _properties.Select(p => new ColumnInfo(name: p.Name, valueType: p.PropertyType)).ToArray();
        }

        public static AsyncPropertyReflectionSource<T> Create(IAsyncEnumerable<T> elements)
        {
            var t = typeof(T);
            if (t == typeof(object)) // Good enough. This will cover the dynamic case but also if someone adds raw object elements which would make no sense anyway.
                throw new ArgumentException("Dynamic objects not supported in " + nameof(PropertyReflectionSource) + ".");

            if (elements is null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            return new AsyncPropertyReflectionSource<T>(elements.GetAsyncEnumerator(), typeof(T));
        }

        public static AsyncPropertyReflectionSource<T> Create(IAsyncEnumerator<T> elements)
        {
            if (elements is null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            return new AsyncPropertyReflectionSource<T>(elements, typeof(T));
        }


        public void Dispose()
        {
            // Nothing to dispose
        }

        public Task CompleteAsync()
        {
            // Nothing to complete in this source
            return Task.CompletedTask;
        }

        public Task<ColumnInfo[]> InitAndGetColumnsAsync()
        {
            return Task.FromResult(_columns);
        }

        public async Task<bool> ReadRowAsync(object[] values)
        {
            if (values.Length != _properties.Length)
                throw new ArgumentException($"Object array of size {values.Length} provided, but {_properties.Length} columns exist");
            
            if (await _elements.MoveNextAsync())
            {
                for (var i = 0; i < _properties.Length; i++)
                {
                    values[i] = _properties[i].GetValue(_elements.Current);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
