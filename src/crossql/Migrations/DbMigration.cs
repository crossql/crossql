using System;
using System.Threading.Tasks;

namespace crossql.Migrations
{
    public abstract class DbMigration : IDbMigration
    {
        protected DbMigration(int migrationVersion)
        {
            MigrationVersion = migrationVersion;
        }

        /// <inheritdoc cref="IDbMigration" />
        /// <remarks>Calling base. is not required</remarks>
        public int MigrationVersion { get; }


        /// <inheritdoc cref="IDbMigration" />
        /// <remarks>Calling base. is not required</remarks>
        /// <remarks>Calling base. is not required</remarks>
        public virtual Task SetupMigration(Database database, IDbProvider provider)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc cref="IDbMigration" />
        /// <remarks>Calling base. is not required</remarks>
        public virtual Task Migrate(Database database, IDbProvider provider)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc cref="IDbMigration" />
        /// <remarks>Calling base. is not required</remarks>
        public virtual Task FinishMigration(Database database, IDbProvider provider)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc cref="IDbMigration" />
        /// <remarks>Calling base. is not required</remarks>
        public virtual Task ClientSetupMigration(Database database, IDbProvider provider)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc cref="IDbMigration" />
        /// <remarks>Calling base. is not required</remarks>
        public virtual Task ClientMigrate(Database database, IDbProvider provider)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc cref="IDbMigration" />
        /// <remarks>Calling base. is not required</remarks>
        public virtual Task ClientFinishMigration(Database database, IDbProvider provider)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc cref="IDbMigration" />
        /// <remarks>Calling base. is not required</remarks>
        public virtual Task ServerSetupMigration(Database database, IDbProvider provider)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc cref="IDbMigration" />
        /// <remarks>Calling base. is not required</remarks>
        public virtual Task ServerMigrate(Database database, IDbProvider provider)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc cref="IDbMigration" />
        /// <remarks>Calling base. is not required</remarks>
        public virtual Task ServerFinishMigration(Database database, IDbProvider provider)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc cref="IDbMigration" />
        /// <remarks>Calling base. is not required</remarks>
        public virtual Task MigrationFailed(Database database, IDbProvider provider, MigrationStep migrationThatFailed, Exception exception)
        {
            return Task.CompletedTask;
        }
    }
}