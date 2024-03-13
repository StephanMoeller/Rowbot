using System.Threading.Tasks;

namespace Rowbot
{
    public interface IAsyncRowSource
    {
        Task<ColumnInfo[]> InitAndGetColumnsAsync();
        Task<bool> ReadRowAsync(object[] values);
        Task CompleteAsync();
    }
}
