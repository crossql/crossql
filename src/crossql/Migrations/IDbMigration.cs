using System;
using System.Collections.Generic;

namespace crossql.Migrations {
    public interface IDbMigration {
        Dictionary<MigrationStep, Action<Database, IDbProvider>> Migration { get; }
        int MigrationVersion { get; }
    }
}