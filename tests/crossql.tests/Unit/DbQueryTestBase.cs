using System.Collections.Generic;
namespace crossql.tests.Unit
{
    public class DbQueryTestBase
    {
        public static IEnumerable<IDbProvider> Repositories()
        {
            var cp = new mssqlserver.DbConnectionProvider(null, null);
            var sqlServerDbProvider = new mssqlserver.DbProvider(cp, "foo");
            var sqliteDbProvider = new sqlite.DbProvider(cp,"foo");

            yield return sqlServerDbProvider;
            yield return sqliteDbProvider;
        }
    }
}