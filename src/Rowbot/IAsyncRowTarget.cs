using System.Threading.Tasks;

namespace Rowbot
{
    public interface IAsyncRowTarget
    {
        Task InitAsync(ColumnInfo[] columns);
        Task WriteRowAsync(object[] values);
        Task CompleteAsync();
    }
}
