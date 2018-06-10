using System.Collections.Generic;
namespace crossql.tests.Unit
{
    public class DbQueryTestBase
    {
        public static IEnumerable<IDbProvider> Repositories()
        {
            var mscp = new mssqlserver.DbConnectionProvider(null, null);
            var litecp = new sqlite.DbConnectionProvider("foo");
            var sqlServerDbProvider = new mssqlserver.DbProvider(mscp, "foo");
            var sqliteDbProvider = new sqlite.DbProvider(litecp);

            yield return sqlServerDbProvider;
            yield return sqliteDbProvider;
        }
    }
}