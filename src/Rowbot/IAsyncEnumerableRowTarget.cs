using System.Threading.Tasks;

namespace Rowbot
{
    public interface IAsyncEnumerableRowTarget<TOutputType>
    {
        Task InitAsync(ColumnInfo[] columns);
        Task<TOutputType> WriteRowAsync(object[] values);
        Task CompleteAsync();
    }
}
