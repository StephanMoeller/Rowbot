using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.Common;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace Rowbot.Sources
{
    public sealed class DataReaderSource : IRowSource, IDisposable
    {
        private readonly IDataReader _dataReader;
        private readonly bool _leaveOpen;

        public DataReaderSource(IDataReader dataReader, bool leaveOpen = false)
        {
            _dataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
            _leaveOpen = leaveOpen;
        }

        public void Complete()
        {
            if (!_leaveOpen)
            {
                _dataReader.Close();
            }
        }

        public void Dispose()
        {
            if (!_leaveOpen)
            {
                _dataReader.Dispose();
            }
        }

        public ColumnInfo[] InitAndGetColumns()
        {
            throw new NotImplementedException(); // Ensure use of column names instead of indexes here!
            //var columnInfos = _dataReader.GetSchemaTable().Rows.Cast<DataRow>().Select(row => new ColumnInfo(name: (string)row[0], valueType: (Type)row[5])).ToArray();
            //return columnInfos;
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
