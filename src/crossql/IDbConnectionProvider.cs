using System.Data;
using System.Threading.Tasks;

namespace crossql
{
    public interface IDbConnectionProvider
    {
        Task<IDbConnection> GetOpenConnection();
    }
}