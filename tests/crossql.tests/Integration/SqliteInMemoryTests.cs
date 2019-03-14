using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using crossql.Migrations;
using crossql.sqlite;
using crossql.tests.Helpers.Migrations;
using crossql.tests.Helpers.Models;
using FluentAssertions;
using NUnit.Framework;
using crossql.Extensions;

namespace crossql.tests.Integration
{
    [TestFixture]
    public class SqliteInMemoryTests
    {

        [Test]
        public async Task ShouldCreateDatabaseOnDiscAndRunProcessesInMemory()
        {
            // setup
            const string dbName = "database.sqlite3";
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var fullDbPath = Path.Combine(path, dbName);
            if(File.Exists(fullDbPath)) File.Delete(fullDbPath);
            
            var diskDb = new DbProvider(new DbConnectionProvider(dbName, s => Path.Combine(path,s)));
            await RunMigrations(diskDb);
            
            // execute
            await diskDb.RunInMemoryTransaction(async trans =>
            {
                for (var i = 0; i < 100; i++)
                {
                    var auto = new Automobile
                    {
                        Vin = i.ToString(),
                        VehicleType = "Car",
                        WheelCount = 4
                    };

                    await trans.CreateOrUpdate(auto);
                }
            });
            var auto10 = await diskDb.Query<Automobile>().Where(a => a.Vin == "10").FirstOrDefaultAsync();
            var allAutos = await diskDb.Query<Automobile>().ToListAsync();
            
            // assert
            allAutos.Count.Should().Be(100);
            auto10.Should().NotBeNull();
            auto10.VehicleType.Should().Be("Car");
            auto10.WheelCount.Should().Be(4);
        }

        private static async Task RunMigrations(IDbProvider dbProvider)
        {
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
}