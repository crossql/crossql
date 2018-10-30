using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using crossql.Extensions;
using Version = crossql.Models.Version;

namespace crossql.Migrations
{
    public class MigrationRunner : IMigrationRunner
    {
        private readonly IDbProvider _dbProvider;

        public MigrationRunner(IDbProvider dbProvider)
        {
            _dbProvider = dbProvider;
        }

        public async Task CreateDatabase()
        {
            // Check if our database exists yet
            var databaseExists = await _dbProvider.CheckIfDatabaseExists().ConfigureAwait(false);
            if (!databaseExists)
                await _dbProvider.CreateDatabase().ConfigureAwait(false);

            await CreateSystemTables().ConfigureAwait(false);
        }

        public async Task DropDatabase()
        {
            // drop the database in the tear down process.
            // this should only be run from the integration tests.
            var databaseExists = await _dbProvider.CheckIfDatabaseExists().ConfigureAwait(false);
            if (databaseExists) await _dbProvider.DropDatabase().ConfigureAwait(false);
        }

        public async Task RunAll(SystemRole systemRole, IEnumerable<IDbMigration> migrations)
        {
            await CreateDatabase().ConfigureAwait(false);

            var orderedMigrations = migrations.OrderBy(m => m.MigrationVersion);

            foreach (var migration in orderedMigrations) await Run(systemRole, migration).ConfigureAwait(false);
        }

        public async Task Run(SystemRole systemRole, IDbMigration migration)
        {
            var databaseVersion = await GetMigrationInformationAsync(migration).ConfigureAwait(false);

            await RunBeforeMigration(migration, databaseVersion, systemRole).ConfigureAwait(false);
            await RunMigration(migration, databaseVersion, systemRole).ConfigureAwait(false);
            await RunAfterMigration(migration, databaseVersion, systemRole).ConfigureAwait(false);
        }

        private async Task RunBeforeMigration(
            IDbMigration migration,
            Version version,
            SystemRole systemRole)
        {
            // Check Actual DatabaseVersion against the migration version
            // Don't run unless this Migrations BeforeMigration has not been run
            if (!version.IsSetupComplete)
            {
                switch (systemRole)
                {
                    case SystemRole.Server:
                        await InternalRunMigration(migration, MigrationStep.ServerSetup, _dbProvider).ConfigureAwait(false);
                        break;
                    case SystemRole.Client:
                        await InternalRunMigration(migration, MigrationStep.ClientSetup, _dbProvider).ConfigureAwait(false);
                        break;
                }

                await InternalRunMigration(migration, MigrationStep.Setup, _dbProvider).ConfigureAwait(false);

                // Update the database version to show the before migration has been run
                version.IsSetupComplete = true;
                await _dbProvider.Query<Version>()
                    .Where(dbv => dbv.VersionNumber == migration.MigrationVersion)
                    .Update(version)
                    .ConfigureAwait(false);
            }
        }

        private async Task RunMigration(
            IDbMigration migration, 
            Version version,
            SystemRole systemRole)
        {
            // Check Actual DatabaseVersion against the migration version
            // Don't run unless this Migrations Migration has not been run
            if (!version.IsMigrationComplete)
            {
                switch (systemRole)
                {
                    case SystemRole.Server:
                        await InternalRunMigration(migration, MigrationStep.ServerMigrate, _dbProvider)
                            .ConfigureAwait(false);
                        break;
                    case SystemRole.Client:
                        await InternalRunMigration(migration, MigrationStep.ClientMigrate, _dbProvider)
                            .ConfigureAwait(false);
                        break;
                }

                await InternalRunMigration(migration, MigrationStep.Migrate, _dbProvider).ConfigureAwait(false);

                // Update the database version to show the before migration has been run
                version.IsMigrationComplete = true;
                await _dbProvider.Query<Version>()
                    .Where(dbv => dbv.VersionNumber == migration.MigrationVersion)
                    .Update(version)
                    .ConfigureAwait(false);
            }
        }

        private async Task RunAfterMigration(IDbMigration migration, Version version,
            SystemRole systemRole)
        {
            // Check Actual DatabaseVersion against the migration version
            // Don't run unless this Migrations AfterMigration has not been run
            if (!version.IsFinishComplete)
            {
                switch (systemRole)
                {
                    case SystemRole.Server:
                        await InternalRunMigration(migration, MigrationStep.ServerFinish, _dbProvider).ConfigureAwait(false);
                        break;
                    case SystemRole.Client:
                        await InternalRunMigration(migration, MigrationStep.ClientFinish, _dbProvider).ConfigureAwait(false);
                        break;
                }

                await InternalRunMigration(migration, MigrationStep.Finish, _dbProvider).ConfigureAwait(false);

                // Update the database version to show the before migration has been run
                version.IsFinishComplete = true;
                await _dbProvider.Query<Version>()
                    .Where(dbv => dbv.VersionNumber == migration.MigrationVersion)
                    .Update(version)
                    .ConfigureAwait(false);
            }
        }
        
        private async Task InternalRunMigration(
            IDbMigration migration, 
            MigrationStep migrationStep,
            IDbProvider dbProvider)
        {
            var failedStep = MigrationStep.Unknown;
            var database = new Database(dbProvider.DatabaseName, dbProvider.Dialect);
            try
            {
                switch (migrationStep)
                {
                    case MigrationStep.Setup:
                        failedStep = MigrationStep.Setup;
                        await migration.SetupMigration(database, dbProvider).ConfigureAwait(false);
                        break;
                    case MigrationStep.Migrate:
                        failedStep = MigrationStep.Migrate;
                        await migration.Migrate(database, dbProvider).ConfigureAwait(false);
                        break;
                    case MigrationStep.Finish:
                        failedStep = MigrationStep.Finish;
                        await migration.FinishMigration(database, dbProvider).ConfigureAwait(false);
                        break;
                    case MigrationStep.ClientSetup:
                        failedStep = MigrationStep.ClientSetup;
                        await migration.ClientSetupMigration(database, dbProvider).ConfigureAwait(false);
                        break;
                    case MigrationStep.ClientMigrate:
                        failedStep = MigrationStep.ClientMigrate;
                        await migration.ClientMigrate(database, dbProvider).ConfigureAwait(false);
                        break;
                    case MigrationStep.ClientFinish:
                        failedStep = MigrationStep.ClientFinish;
                        await migration.ClientFinishMigration(database, dbProvider).ConfigureAwait(false);
                        break;
                    case MigrationStep.ServerSetup:
                        failedStep = MigrationStep.ServerSetup;
                        await migration.ServerSetupMigration(database, dbProvider).ConfigureAwait(false);
                        break;
                    case MigrationStep.ServerMigrate:
                        failedStep = MigrationStep.ServerMigrate;
                        await migration.ServerMigrate(database, dbProvider).ConfigureAwait(false);
                        break;
                    case MigrationStep.ServerFinish:
                        failedStep = MigrationStep.ServerFinish;
                        await migration.ServerFinishMigration(database, dbProvider).ConfigureAwait(false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(migrationStep), migrationStep, $"An invalid {nameof(MigrationStep)} was passed to the {nameof(MigrationRunner)}");
                }

                var dbCommand = database.ToString();
                if (!string.IsNullOrWhiteSpace(dbCommand))
                    await _dbProvider.ExecuteNonQuery(dbCommand);
            }
            catch (Exception exception)
            {
                await migration.MigrationFailed(database, dbProvider, failedStep, exception);
                throw;
            }
        }

        private async Task<Version> GetMigrationInformationAsync(IDbMigration migration)
        {
            var databaseVersion = await _dbProvider.Query<Version>()
                .Where(dbv => dbv.VersionNumber == migration.MigrationVersion)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (databaseVersion != null) return databaseVersion;

            databaseVersion = new Version
            {
                VersionNumber = migration.MigrationVersion,
                MigrationDate = DateTimeOffset.Now
            };
            await _dbProvider.Create(databaseVersion).ConfigureAwait(false);

            return databaseVersion;
        }

        private async Task CreateSystemTables()
        {
            // Check if DatabaseVersion table is setup, if not, create it.
            var versionTableExists = await _dbProvider.CheckIfTableExists(WellKnownValues.VersionTableName).ConfigureAwait(false);
            if (!versionTableExists)
            {
                var database = new Database(_dbProvider.DatabaseName, _dbProvider.Dialect);

                var dbVersionTable = database.AddTable(WellKnownValues.VersionTableName);
                dbVersionTable.AddColumn(nameof(Version.VersionNumber), typeof(int)).PrimaryKey().Clustered().NotNullable();
                dbVersionTable.AddColumn(nameof(Version.MigrationDate), typeof(DateTimeOffset)).NotNullable();
                dbVersionTable.AddColumn(nameof(Version.IsSetupComplete), typeof(bool)).NotNullable(true);
                dbVersionTable.AddColumn(nameof(Version.IsMigrationComplete), typeof(bool)).NotNullable(true);
                dbVersionTable.AddColumn(nameof(Version.IsFinishComplete), typeof(bool)).NotNullable(true);

                var schema = database.ToString();
                await _dbProvider.ExecuteNonQuery(schema).ConfigureAwait(false);
            }
        }
    }
}