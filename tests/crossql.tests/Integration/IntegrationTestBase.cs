using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using crossql.Config;
using crossql.Migrations;
using crossql.tests.Helpers.Migrations;
using crossql.tests.Helpers.Models;
using NUnit.Framework;
using SqlDbConnectionProvider = crossql.mssqlserver.DbConnectionProvider;
using SqlDbProvider = crossql.mssqlserver.DbProvider;
using SqliteDbProvider = crossql.sqlite.DbProvider;
namespace crossql.tests.Integration
{
    public abstract class IntegrationTestBase
    {
        private static readonly string _testDbName = ConfigurationManager.AppSettings["databaseName"];

        public static IEnumerable<IDbProvider> DbProviders()
        {
            return SqliteOnly().Concat(MsSqlOnly());
        }

        public static IEnumerable<IDbProvider> SqliteOnly()
        {
            var litecp = new sqlite.DbConnectionProvider($"{_testDbName}.sqlite3");
            yield return new SqliteDbProvider(litecp);
        }

        public static IEnumerable<IDbProvider> MsSqlOnly()
        {
            var sqlServerConnection = ConfigurationManager.ConnectionStrings["databaseConnection"];

            var sqlDbConnectionProvider = new SqlDbConnectionProvider(
                sqlServerConnection.ConnectionString,
                sqlServerConnection.ProviderName);            
            yield return new SqlDbProvider(sqlDbConnectionProvider, _testDbName, SetConfig);
        }

        [OneTimeSetUp]
        public async Task Setup()
        {
            foreach (var dbProvider in DbProviders())
            {
                Trace.WriteLine(TraceObjectGraphInfo(dbProvider));
                var migrationRunner = new MigrationRunner(dbProvider);

                // drop the database before running the tests again
                await migrationRunner.DropDatabase();
                
                await migrationRunner.RunAll(SystemRole.Server, new List<IDbMigration>
                {
                    new Migration001(),
                    new Migration002(),
                    new Migration003(),
                    new Migration004()
                });
            }
        }

        protected string TraceObjectGraphInfo(IDbProvider dbProvider)
        {
            var dbProviderFriendlyName = dbProvider.GetType().ToString()
                                                   .Replace("crossql.", "'")
                                                   .Replace(".", "' ");

            return "Tested against the " + dbProviderFriendlyName;
        }

        private static void SetConfig(DbConfiguration cfg)
        {
            cfg.Configure<Automobile>(opts => opts.SetPrimaryKey(a => a.Vin));
        }
    }
}