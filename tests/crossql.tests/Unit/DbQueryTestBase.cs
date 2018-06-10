using System.Collections.Generic;
namespace crossql.tests.Unit
{
    public class DbQueryTestBase
    {
        public static IEnumerable<IDbProvider> Repositories()
        {
            const string dbName = "foo";
            var mssqlConnectionProvider = new mssqlserver.DbConnectionProvider(null, null);
            var sqliteConnectionProvider = new sqlite.DbConnectionProvider(dbName);

            var mssqlDbProvider = new mssqlserver.DbProvider(mssqlConnectionProvider, dbName);
            var sqliteDbProvider = new sqlite.DbProvider(sqliteConnectionProvider);

            yield return mssqlDbProvider;
            yield return sqliteDbProvider;
        }
    }
}