using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Rowbot.Sources
{
    public class PropertyReflectionSource : RowSource
    {
        private readonly IEnumerator _elements;
        private PropertyInfo[] _properties;
        private ColumnInfo[] _columns;
        private PropertyReflectionSource(IEnumerable elements, Type elementType)
        {
            if (elements is null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            if (elementType is null)
            {
                throw new ArgumentNullException(nameof(elementType));
            }

            _elements = elements.GetEnumerator();
            _properties = elementType.GetProperties();
            _columns = _properties.Select(p => new ColumnInfo(name: p.Name, valueType: p.PropertyType)).ToArray();
        }

        public static PropertyReflectionSource Create<T>(IEnumerable<T> elements)
        {
            if (elements is null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            return new PropertyReflectionSource(elements, typeof(T));
        }

        public override void Dispose()
        {
            // Nothing to dispose
        }

        protected override void OnComplete()
        {
            // Nothing to complete in this source
        }

        protected override ColumnInfo[] OnInitAndGetColumns()
        {
            return _columns;
        }

        protected override bool OnReadRow(object[] values)
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
