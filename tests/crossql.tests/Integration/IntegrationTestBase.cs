using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using crossql.Config;
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
        public static IEnumerable<IDbProvider> DbProviders()
        {
            var testDbName = ConfigurationManager.AppSettings["databaseName"];
            var sqlServerConnection = ConfigurationManager.ConnectionStrings["databaseConnection"];

            var sqlDbConnectionProvider = new SqlDbConnectionProvider(
                sqlServerConnection.ConnectionString,
                sqlServerConnection.ProviderName);

            yield return new SqlDbProvider(sqlDbConnectionProvider, testDbName, SetConfig);
            yield return new SqliteDbProvider($"{testDbName}.sqlite3", SetConfig);
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
                
                await migrationRunner.RunAll(SystemRole.Server, new List<CrossqlMigration>
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
            cfg.Configure<AutomobileModel>(opts => opts.SetPrimaryKey(a => a.Vin));
        }
    }
}