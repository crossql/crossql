using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace crossql
{
    public abstract class CrossqlMigration
    {
        protected CrossqlMigration(int migrationVersion)
        {
            MigrationVersion = migrationVersion;
            Migration = new Dictionary<MigrationStep, Action<Database, IDbProvider>>();
        }

        public Dictionary<MigrationStep, Action<Database, IDbProvider>> Migration { get; }

        internal int MigrationVersion { get; }

        internal async Task RunOrderedMigrationAsync(MigrationStep key, IDbProvider dbProvider)
        {
            if (!Migration.ContainsKey(key))
                return;
            var database = new Database(dbProvider.DatabaseName, dbProvider.Dialect);
            Migration[key].Invoke(database, dbProvider);
            var dbCommand = database.ToString();
            if(!string.IsNullOrWhiteSpace(dbCommand))
                await dbProvider.ExecuteNonQuery(dbCommand);
        }
    }
}