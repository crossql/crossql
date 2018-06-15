using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using crossql.Extensions;
using crossql.Models;

namespace crossql.Migrations
{
    public class MigrationRunner : IMigrationRunner
    {
        private readonly IDbProvider _dbProvider;
        private SystemRole _systemRole;

        public MigrationRunner(IDbProvider dbProvider)
        {
            _dbProvider = dbProvider;
        }

        public async Task CreateDatabase()
        {
            // Check if our database exists yet
            if (! await _dbProvider.CheckIfDatabaseExists().ConfigureAwait(false))
            {
                await _dbProvider.CreateDatabase().ConfigureAwait(false);
            }

            // Check if DatabaseVersion table is setup, if not, create it.
            if (!await _dbProvider.CheckIfTableExists(WellKnownValues.VersionTableName).ConfigureAwait(false))
            {
                var database = new Database(_dbProvider.DatabaseName, _dbProvider.Dialect);

                var dbVersionTable = database.AddTable(WellKnownValues.VersionTableName);
                dbVersionTable.AddColumn("VersionNumber", typeof (int)).PrimaryKey().Clustered().NotNullable();
                dbVersionTable.AddColumn("MigrationDate", typeof (DateTime)).NotNullable();
                dbVersionTable.AddColumn("IsBeforeMigrationComplete", typeof (bool)).NotNullable(true);
                dbVersionTable.AddColumn("IsMigrationComplete", typeof (bool)).NotNullable(true);
                dbVersionTable.AddColumn("IsAfterMigrationComplete", typeof (bool)).NotNullable(true);

                await _dbProvider.ExecuteNonQuery(database.ToString()).ConfigureAwait(false);
            }
            else
            {
                // Check if the new fields have bee added to the DatabaseVersion table yet, if not add them.
                if (!await _dbProvider.CheckIfTableColumnExists(WellKnownValues.VersionTableName, "IsBeforeMigrationComplete").ConfigureAwait(false))
                {
                    var database = new Database(_dbProvider.DatabaseName, _dbProvider.Dialect);

                    var dbVersionTable = database.UpdateTable(WellKnownValues.VersionTableName);
                    dbVersionTable.AddColumn("IsBeforeMigrationComplete", typeof (bool)).NotNullable(true);
                    dbVersionTable.AddColumn("IsMigrationComplete", typeof (bool)).NotNullable(true);
                    dbVersionTable.AddColumn("IsAfterMigrationComplete", typeof (bool)).NotNullable(true);

                    await _dbProvider.ExecuteNonQuery(database.ToString()).ConfigureAwait(false);
                }
            }
        }

        public async Task DropDatabase()
        {
            // drop the database in the tear down process.
            // this should only be run from the integration tests.
            if (await _dbProvider.CheckIfDatabaseExists().ConfigureAwait(false))
            {
                await _dbProvider.DropDatabase().ConfigureAwait(false);
            }
        }

        public async Task RunAll(SystemRole systemRole, IList<IDbMigration> migrations)
        {
            _systemRole = systemRole;

            await CreateDatabase().ConfigureAwait(false);

            var orderedMigrations = migrations.OrderBy(m => m.GetType().Name);

            foreach (var migration in orderedMigrations)
            {
                var databaseVersion = await GetMigrationInformationAsync(migration).ConfigureAwait(false);

                await RunBeforeMigration(migration,  databaseVersion).ConfigureAwait(false);
                await RunMigration(migration,  databaseVersion).ConfigureAwait(false);
                await RunAfterMigration(migration,  databaseVersion).ConfigureAwait(false);
            }
        }

        public async Task Run(SystemRole systemRole, IDbMigration migration)
        {
            _systemRole = systemRole;

            var databaseVersion = await GetMigrationInformationAsync(migration).ConfigureAwait(false);

            await RunBeforeMigration(migration,  databaseVersion).ConfigureAwait(false);
            await RunMigration(migration,  databaseVersion).ConfigureAwait(false);
            await RunAfterMigration(migration,  databaseVersion).ConfigureAwait(false);
        }

        private async Task RunBeforeMigration(IDbMigration migration, DatabaseVersionModel databaseVersion)
        {
            // Check Actual DatabaseVersion against the migration version
            // Don't run unless this Migrations BeforeMigration has not been run
            if (databaseVersion.IsBeforeMigrationComplete == false)
            {
                // Before Migrate

                await migration.RunOrderedMigration(MigrationStep.Setup, _dbProvider).ConfigureAwait(false);

                if (_systemRole == SystemRole.Server)
                {
                    await migration.RunOrderedMigration(MigrationStep.ServerSetup, _dbProvider).ConfigureAwait(false);
                }
                if (_systemRole == SystemRole.Client)
                {
                    await migration.RunOrderedMigration(MigrationStep.ClientSetup, _dbProvider).ConfigureAwait(false);
                }

                // Update the database version to show the before migration has been run
                databaseVersion.IsBeforeMigrationComplete = true;
                await _dbProvider.Query<DatabaseVersionModel>()
                    .Where(dbv => dbv.VersionNumber == migration.MigrationVersion)
                    .Update(databaseVersion)
                    .ConfigureAwait(false);
            }
        }

        private async Task RunMigration(IDbMigration migration, DatabaseVersionModel databaseVersion)
        {
            // Check Actual DatabaseVersion against the migration version
            // Don't run unless this Migrations Migration has not been run
            if (databaseVersion.IsMigrationComplete == false)
            {
                await migration.RunOrderedMigration(MigrationStep.Migrate, _dbProvider).ConfigureAwait(false);

                switch (_systemRole) {
                    case SystemRole.Server:
                        await migration.RunOrderedMigration(MigrationStep.ServerMigrate, _dbProvider).ConfigureAwait(false);
                        break;
                    case SystemRole.Client:
                        await migration.RunOrderedMigration(MigrationStep.ClientMigrate, _dbProvider).ConfigureAwait(false);
                        break;
                }

                // Update the database version to show the migration has been run
                databaseVersion.IsMigrationComplete = true;
                await _dbProvider.Query<DatabaseVersionModel>()
                    .Where(dbv => dbv.VersionNumber == migration.MigrationVersion)
                    .Update(databaseVersion)
                    .ConfigureAwait(false);
            }

        }

        private async Task RunAfterMigration(IDbMigration migration, DatabaseVersionModel databaseVersion)
        {
            // Check Actual DatabaseVersion against the migration version
            // Don't run unless the MigrationVersion is 1 more than DatabaseVersion
            if (databaseVersion.IsAfterMigrationComplete == false)
            {
                await migration.RunOrderedMigration(MigrationStep.Finish, _dbProvider).ConfigureAwait(false);

                if (_systemRole == SystemRole.Server)
                {
                    await migration.RunOrderedMigration(MigrationStep.ServerFinish, _dbProvider).ConfigureAwait(false);
                }
                if (_systemRole == SystemRole.Client)
                {
                    await migration.RunOrderedMigration(MigrationStep.ClientFinish, _dbProvider).ConfigureAwait(false);
                }

                // Update the database version to show the after migration has been run
                databaseVersion.IsAfterMigrationComplete = true;
                await _dbProvider.Query<DatabaseVersionModel>()
                    .Where(dbv => dbv.VersionNumber == migration.MigrationVersion)
                    .Update(databaseVersion)
                    .ConfigureAwait(false);
            }
        }

        private async Task<DatabaseVersionModel> GetMigrationInformationAsync(IDbMigration migration)
        {
            var databaseVersions = await _dbProvider.Query<DatabaseVersionModel>()
                .Where(dbv => dbv.VersionNumber == migration.MigrationVersion)
                .OrderBy(v => v.VersionNumber, OrderDirection.Descending)
                .Select()
                .ConfigureAwait(false);
            var databaseVersion = databaseVersions.FirstOrDefault();

            if (databaseVersion == null)
            {
                databaseVersion = new DatabaseVersionModel
                {
                    VersionNumber = migration.MigrationVersion,
                    MigrationDate = DateTime.UtcNow
                };
                await _dbProvider.Create(databaseVersion).ConfigureAwait(false);
            }

            return databaseVersion;
        }
    }
}