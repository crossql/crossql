using System.Collections.Generic;
using System.Threading.Tasks;

namespace crossql
{
    public interface IMigrationRunner
    {
        Task CreateDatabase();
        Task DropDatabase();
        Task Run(SystemRole systemRole, CrossqlMigration migrations);
        Task RunAll(SystemRole systemRole, IList<CrossqlMigration> migrations);
    }
}