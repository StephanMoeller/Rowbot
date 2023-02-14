using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.Common;

namespace Rowbot.Sources
{
    public class DataReaderSource : RowSource
    {
        private readonly IDataReader _dataReader;
        private string[] _columnNames;
        public DataReaderSource(IDataReader dataReader)
        {
            _dataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
            _columnNames = _dataReader.GetSchemaTable().Rows.Cast<DataRow>().Select(row => row[0].ToString()).ToArray();
        }

        public override void Dispose()
        {
            _dataReader?.Dispose();
        }

        protected override void OnComplete()
        {
            // Nothing to complete in this source
        }

        protected override ColumnInfo[] OnInitAndGetColumns()
        {
            var columnInfos = _dataReader.GetSchemaTable().Rows.Cast<DataRow>().Select(row => new ColumnInfo(name: (string)row[0], valueType: (Type)row[5])).ToArray();
            return columnInfos;
        }

        protected override bool OnReadRow(object[] values)
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
