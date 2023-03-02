using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Rowbot.Sources
{
    public class PropertyReflectionSource : IRowSource
    {
        private readonly IEnumerator _elements;
        private readonly PropertyInfo[] _properties;
        private readonly ColumnInfo[] _columns;
        private PropertyReflectionSource(IEnumerator elements, Type elementType)
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

        public static PropertyReflectionSource Create<T>(IEnumerable<T> elements)
        {
            if (elements is null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            return new PropertyReflectionSource(elements.GetEnumerator(), typeof(T));
        }

        public static PropertyReflectionSource Create<T>(IEnumerator<T> elements)
        {
            if (elements is null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            return new PropertyReflectionSource(elements, typeof(T));
        }


        public void Dispose()
        {
            // Nothing to dispose
        }

        public void Complete()
        {
            // Nothing to complete in this source
        }

        public ColumnInfo[] InitAndGetColumns()
        {
            return _columns;
        }

        public bool ReadRow(object[] values)
        {
            if (values.Length != _properties.Length)
                throw new ArgumentException($"Object array of size {values.Length} provided, but {_properties.Length} columns exist");
            if (_elements.MoveNext())
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
