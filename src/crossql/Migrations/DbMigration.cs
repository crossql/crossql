using System;
using System.Collections.Generic;

namespace crossql.Migrations
{
    public abstract class DbMigration : IDbMigration
    {
        protected DbMigration(int migrationVersion)
        {
            MigrationVersion = migrationVersion;
            Migration = new Dictionary<MigrationStep, Action<Database, IDbProvider>>();
        }

        public Dictionary<MigrationStep, Action<Database, IDbProvider>> Migration { get; }

        public int MigrationVersion { get; }
    }
}