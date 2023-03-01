using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.Common;

namespace Rowbot.Sources
{
    public class DataReaderSource : IRowSource
    {
        private readonly IDataReader _dataReader;
        private string[] _columnNames;
        public DataReaderSource(IDataReader dataReader)
        {
            _dataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
            _columnNames = _dataReader.GetSchemaTable().Rows.Cast<DataRow>().Select(row => row[0].ToString()).ToArray();
        }

        public void Dispose()
        {
            _dataReader?.Dispose();
        }

        public void Complete()
        {
            // Nothing to complete in this source
        }

        public ColumnInfo[] InitAndGetColumns()
        {
            var columnInfos = _dataReader.GetSchemaTable().Rows.Cast<DataRow>().Select(row => new ColumnInfo(name: (string)row[0], valueType: (Type)row[5])).ToArray();
            return columnInfos;
        }

        public bool ReadRow(object[] values)
        {
            if (_dataReader.Read())
            {
                _dataReader.GetValues(values);

                // Replace DBNull with null
                for (var i = 0; i < values.Length; i++)
                {
                    if (values[i] == DBNull.Value)
                    {
                        values[i] = null;
                    }
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
