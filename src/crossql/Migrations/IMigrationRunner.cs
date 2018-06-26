using System.Collections.Generic;
using System.Threading.Tasks;

namespace crossql.Migrations
{
    public interface IMigrationRunner
    {
        Task CreateDatabase();
        Task DropDatabase();
        Task Run(SystemRole systemRole, IDbMigration migrations);
        Task RunAll(SystemRole systemRole, IEnumerable<IDbMigration> migrations);
    }
}