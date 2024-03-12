using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rowbot.Targets
{
    public class AsyncPropertyReflectionTarget<T> : IAsyncEnumerableRowTarget<T> where T : new()
    {
        private PropertyInfo[] _propertiesByColumnIndex;
        private int[] _supportedIndexes;
        private bool[] _throwIfSourceValuesIsNullByIndex;
        public AsyncPropertyReflectionTarget()
        {
            // Sanity check that there is anything to write on this object
            var allWritableProperties = typeof(T).GetProperties().Where(p => p.CanWrite).ToArray();
            if (allWritableProperties.Length == 0)
                throw new ArgumentException($"No writable properties found on type {typeof(T).FullName}");
        }

        public Task CompleteAsync()
        {
            // Nothing to complete in this source
            return Task.CompletedTask;
        }

        public Task InitAsync(ColumnInfo[] columns)
        {
            _propertiesByColumnIndex = new PropertyInfo[columns.Length];
            _throwIfSourceValuesIsNullByIndex = new bool[columns.Length];
            var supportedColumnIndexes = new List<int>();
            var allProperties = typeof(T).GetProperties();
            for (var i = 0; i < columns.Length; i++)
            {
                var column = columns[i];
                var propertyInfoOrNull = FindPropertyOrNull(columnInfo: column, columnIndex: i, allProperties: allProperties);
                if (propertyInfoOrNull != null)
                {
                    _propertiesByColumnIndex[i] = propertyInfoOrNull;
                    supportedColumnIndexes.Add(i);
                }
            }

            _supportedIndexes = supportedColumnIndexes.ToArray();

            return Task.CompletedTask;
        }

        private PropertyInfo FindPropertyOrNull(ColumnInfo columnInfo, int columnIndex, PropertyInfo[] allProperties)
        {
            var matchingProperties = allProperties.Where(p => p.Name.Equals(columnInfo.Name, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matchingProperties.Length > 1)
                throw new InvalidOperationException($"The type {typeof(T).FullName} has multiple properties matching column name {columnInfo.Name} when compared case insensitive. This is not supported.");
            var property = matchingProperties.SingleOrDefault();
            if (property == null)
            {
                // No property matches
                return null;
            }

            if (!property.CanWrite)
            {
                // No setter on property
                return null;
            }

            if (columnInfo.ValueType == property.PropertyType)
                return property; // Exact type match

            // Types are not exact matches. If property has a nullable value and input has a non-nullable, then this should be allowed
            var propertyUnderlayingTypeOrNull = Nullable.GetUnderlyingType(property.PropertyType);
            if (propertyUnderlayingTypeOrNull != null && columnInfo.ValueType == propertyUnderlayingTypeOrNull)
            {
                return property;
            }

            var columnUnderlayingTypeOrNull = Nullable.GetUnderlyingType(columnInfo.ValueType);
            if (columnUnderlayingTypeOrNull != null && columnUnderlayingTypeOrNull == property.PropertyType)
            {
                // Column type is nullable, property is not-nullable. This should only break if actual null value is copied
                _throwIfSourceValuesIsNullByIndex[columnIndex] = true;
                return property;
            }

            throw new TypeMismatchException($"Column with name {columnInfo.Name} at index {columnIndex} is of type {columnInfo.ValueType.FullName} but matching property on type {typeof(T).FullName} is of type {property.PropertyType.FullName}");
        }

        public Task<T> WriteRowAsync(object[] values)
        {
            var element = new T();
            for (var i = 0; i < _supportedIndexes.Length; i++)
            {
                var index = _supportedIndexes[i];
                var value = values[index];
                var property = _propertiesByColumnIndex[index];
                if (value == null && _throwIfSourceValuesIsNullByIndex[index])
                    throw new ArgumentNullException($"Property {property.Name} on type {typeof(T).FullName} cannot be set to NULL");
                property.SetValue(element, value);
            }
            
            return Task.FromResult(element);
        }
    }
}
