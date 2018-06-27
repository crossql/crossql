using System.Collections.Generic;
using System.Threading.Tasks;

namespace crossql.Migrations
{
    public interface IMigrationRunner
    {
        /// <summary>
        /// Create a database if one doesn't already exist. 
        /// </summary>
        Task CreateDatabase();
        
        /// <summary>
        /// Drops a database if it exists
        /// </summary>
        /// <returns>Task</returns>
        Task DropDatabase();
        
        /// <summary>
        /// Run a single migration
        /// </summary>
        /// <param name="systemRole">defines if the migration is to be run on a <see cref="SystemRole.Client"/> or <see cref="SystemRole.Server"/>. Shared migrations will still run regardless.</param>
        /// <param name="migration">migration to be run</param>
        /// <returns>Task</returns>
        Task Run(SystemRole systemRole, IDbMigration migration);
        
        /// <summary>
        /// Run a collection of migrations. Migrations will be ordered by their <see cref="IDbMigration.MigrationVersion"/>
        /// </summary>
        /// <param name="systemRole">defines if the migration is to be run on a <see cref="SystemRole.Client"/> or <see cref="SystemRole.Server"/>. Shared migrations will still run regardless.</param>
        /// <param name="migrations">Collection of migrations to be run</param>
        /// <returns>Task</returns>
        Task RunAll(SystemRole systemRole, IEnumerable<IDbMigration> migrations);
    }
}