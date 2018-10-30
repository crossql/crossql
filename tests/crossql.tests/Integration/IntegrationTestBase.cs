﻿using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using crossql.Config;
using crossql.Migrations;
using crossql.sqlite;
using crossql.tests.Helpers.Migrations;
using crossql.tests.Helpers.Models;
using NUnit.Framework;
using SqlServerDbConnectionProvider = crossql.mssqlserver.DbConnectionProvider;
using MySqlDbConnectionProvider = crossql.mysql.DbConnectionProvider;
using SqliteDbConnectionProvider = crossql.sqlite.DbConnectionProvider;
using SqlServerDbProvider = crossql.mssqlserver.DbProvider;
using SqliteDbProvider = crossql.sqlite.DbProvider;
using MySqlDbProvider = crossql.mysql.DbProvider;

namespace crossql.tests.Integration
{
    public abstract class IntegrationTestBase
    {
        private static readonly string _testDbName = ConfigurationManager.AppSettings["databaseName"];
        private static readonly ConnectionStringSettings _mySqlSettings = ConfigurationManager.ConnectionStrings["mySqlConnection"];
        private static readonly ConnectionStringSettings _sqlServerSettings = ConfigurationManager.ConnectionStrings["sqlServerConnection"];

        protected static IEnumerable<IDbProvider> DbProviders => new[]
        {
            SqliteOnly,
            MsSqlOnly,
            MySqlOnly
        };

        private static IDbProvider SqliteOnly => new SqliteDbProvider(new SqliteDbConnectionProvider(
            $"{_testDbName}.sqlite3", SqliteSettings.Default),SetConfig);

        private static IDbProvider MsSqlOnly => new SqlServerDbProvider(
            new SqlServerDbConnectionProvider(_sqlServerSettings.ConnectionString, _sqlServerSettings.ProviderName),
            _testDbName, SetConfig);

        private static IDbProvider MySqlOnly => new MySqlDbProvider(
            new MySqlDbConnectionProvider(_mySqlSettings.ConnectionString, _mySqlSettings.ProviderName), _testDbName,
            SetConfig);

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

        protected static string TraceObjectGraphInfo(IDbProvider dbProvider)
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