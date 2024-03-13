using System;
using System.Linq;
using System.Data;
using System.Threading.Tasks;

namespace Rowbot.Sources
{
    public sealed class AsyncDataReaderSource : IAsyncRowSource, IDisposable
    {
        private readonly IDataReader _dataReader;
        private readonly bool _leaveOpen;

        public AsyncDataReaderSource(IDataReader dataReader, bool leaveOpen = false)
        {
            _dataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
            _leaveOpen = leaveOpen;
        }
        
        public Task CompleteAsync()
        {
            if (!_leaveOpen)
            {
                _dataReader.Close();
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (!_leaveOpen)
            {
                _dataReader.Dispose();
            }
        }

        public Task<ColumnInfo[]> InitAndGetColumnsAsync()
        {
            var columnInfos = _dataReader.GetSchemaTable().Rows.Cast<DataRow>().Select(row => new ColumnInfo(name: (string)row["ColumnName"], valueType: (Type)row["DataType"])).ToArray();
            return Task.FromResult(columnInfos);
        }

        public Task<bool> ReadRowAsync(object[] values)
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
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
    }
}
