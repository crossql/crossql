using System;
using System.Threading.Tasks;

namespace crossql.Migrations {
    public interface IDbMigration {
        
        /// <summary>
        /// Version of the migration to run.
        /// </summary>
        int MigrationVersion { get; }
        
        /// <summary>
        /// Runs on both client and server before a migration begins
        /// </summary>
        /// <param name="database">Instance of <see cref="Database"/> on which to migrate against</param>
        /// <param name="provider">Instance of<see cref="IDbProvider"/> that you can use for seeding data.</param>
        Task SetupMigration(Database database, IDbProvider provider) ;
        
        /// <summary>
        /// Primary migration that runs on both client and server
        /// </summary>
        /// <param name="database">Instance of <see cref="Database"/> on which to migrate against</param>
        /// <param name="provider">Instance of<see cref="IDbProvider"/> that you can use for seeding data.</param>
        Task Migrate(Database database, IDbProvider provider) ;
        
        /// <summary>
        /// Runs on both client and server after a migration completes
        /// </summary>
        /// <param name="database">Instance of <see cref="Database"/> on which to migrate against</param>
        /// <param name="provider">Instance of<see cref="IDbProvider"/> that you can use for seeding data.</param>
        Task FinishMigration(Database database, IDbProvider provider) ;

        /// <summary>
        /// Runs on the client before a migration begins
        /// </summary>
        /// <param name="database">Instance of <see cref="Database"/> on which to migrate against</param>
        /// <param name="provider">Instance of<see cref="IDbProvider"/> that you can use for seeding data.</param>
        Task ClientSetupMigration(Database database, IDbProvider provider) ;
        
        /// <summary>
        /// Primary migration that runs on the client
        /// </summary>
        /// <param name="database">Instance of <see cref="Database"/> on which to migrate against</param>
        /// <param name="provider">Instance of<see cref="IDbProvider"/> that you can use for seeding data.</param>
        Task ClientMigrate(Database database, IDbProvider provider) ;
        
        /// <summary>
        /// Runs on the client after a migration completes
        /// </summary>
        /// <param name="database">Instance of <see cref="Database"/> on which to migrate against</param>
        /// <param name="provider">Instance of<see cref="IDbProvider"/> that you can use for seeding data.</param>
        Task ClientFinishMigration(Database database, IDbProvider provider) ;

        /// <summary>
        /// Runs on the server before a migration begins
        /// </summary>
        /// <param name="database">Instance of <see cref="Database"/> on which to migrate against</param>
        /// <param name="provider">Instance of<see cref="IDbProvider"/> that you can use for seeding data.</param>
        Task ServerSetupMigration(Database database, IDbProvider provider) ;
        
        /// <summary>
        /// Primary migration that runs on the client
        /// </summary>
        /// <param name="database">Instance of <see cref="Database"/> on which to migrate against</param>
        /// <param name="provider">Instance of<see cref="IDbProvider"/> that you can use for seeding data.</param>
        Task ServerMigrate(Database database, IDbProvider provider) ;
        
        /// <summary>
        /// Runs on the server after a migration completes
        /// </summary>
        /// <param name="database">Instance of <see cref="Database"/> on which to migrate against</param>
        /// <param name="provider">Instance of<see cref="IDbProvider"/> that you can use for seeding data.</param>
        Task ServerFinishMigration(Database database, IDbProvider provider) ;

        /// <summary>
        /// Runs when a server or client migration fails.
        /// </summary>
        /// <param name="database">Instance of <see cref="Database"/> on which to migrate against</param>
        /// <param name="provider">Instance of<see cref="IDbProvider"/> that you can use for seeding data.</param>
        /// <param name="migrationThatFailed">Indicates which migration step failed</param>
        /// <param name="exception">Exception thrown by the failed migration step</param>
        Task MigrationFailed(Database database, IDbProvider provider,MigrationStep migrationThatFailed, Exception exception) ;
    }
}