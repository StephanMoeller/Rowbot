using System.Data;

namespace Rowbot.Sources
{
    public class DataTableSource : DataReaderSource
    {
        public DataTableSource(DataTable dataTable) : base(dataTable.CreateDataReader()) { }
    }
}
