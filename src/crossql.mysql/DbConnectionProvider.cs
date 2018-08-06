using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace crossql.mysql
{
    public class DbConnectionProvider : IDbConnectionProvider
    {
        private readonly string _connectionProviderName;
        private readonly string _connectionString;

        public DbConnectionProvider(string connectionString, string connectionProviderName)
        {
            _connectionString = connectionString;
            _connectionProviderName = connectionProviderName;
        }

        public async Task<IDbConnection> GetOpenConnection()
        {
            var connection = new MySqlConnection {ConnectionString = _connectionString};

            await  connection.OpenAsync().ConfigureAwait(false);

            return connection;
        }
    }
}