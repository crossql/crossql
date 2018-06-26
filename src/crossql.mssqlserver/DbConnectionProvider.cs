using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace crossql.mssqlserver
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
            var connection = new SqlConnection {ConnectionString = _connectionString};

            await  connection.OpenAsync().ConfigureAwait(false);

            return connection;
        }
    }
}