using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using crossql.Config;
using crossql.Migrations;
using crossql.sqlite;
using crossql.tests.Helpers.Migrations;
using crossql.tests.Helpers.Models;
using NUnit.Framework;
using SqlDbConnectionProvider = crossql.mssqlserver.DbConnectionProvider;
using MySqlConnectionProvider = crossql.mysql.DbConnectionProvider;
using SqlDbProvider = crossql.mssqlserver.DbProvider;
using SqliteDbProvider = crossql.sqlite.DbProvider;
using MySqlDbProvider = crossql.mysql.DbProvider;
namespace crossql.tests.Integration
{
    public abstract class IntegrationTestBase
    {
        private static readonly string _testDbName = ConfigurationManager.AppSettings["databaseName"];

        protected static IEnumerable<IDbProvider> DbProviders => new []
        {
            //SqliteOnly,
            //MsSqlOnly,
            MySqlOnly
        };

        private static IDbProvider SqliteOnly
        {
            get{
                var litecp = new sqlite.DbConnectionProvider($"{_testDbName}.sqlite3", SqliteSettings.Default);
                return new SqliteDbProvider(litecp);
            }
        }

        private static IDbProvider MsSqlOnly
        {
            get{
                var sqlServerConnection = ConfigurationManager.ConnectionStrings["sqlServerConnection"];

                var sqlDbConnectionProvider = new SqlDbConnectionProvider(
                    sqlServerConnection.ConnectionString,
                    sqlServerConnection.ProviderName);
                return new SqlDbProvider(sqlDbConnectionProvider, _testDbName, SetConfig);
            }
        }

        private static IDbProvider MySqlOnly
        {
            get
            {
                var mysqlConnection = ConfigurationManager.ConnectionStrings["mySqlConnection"];

                var mySqlDbConnectionProvider = new MySqlConnectionProvider(
                    mysqlConnection.ConnectionString,
                    mysqlConnection.ProviderName);
                return new MySqlDbProvider(mySqlDbConnectionProvider, _testDbName, SetConfig);
            }
        }

        [OneTimeSetUp]
        public async Task Setup()
        {
            foreach (var dbProvider in DbProviders)
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