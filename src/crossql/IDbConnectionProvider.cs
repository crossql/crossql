using System.Data;
using System.Threading.Tasks;

namespace crossql
{
    public interface IDbConnectionProvider
    {
        /// <summary>
        /// Creates a new <see cref="IDbConnection"/> and opens it.
        /// </summary>
        /// <returns>returns an open  <see cref="IDbConnection"/></returns>
        Task<IDbConnection> GetOpenConnection();
    }
}