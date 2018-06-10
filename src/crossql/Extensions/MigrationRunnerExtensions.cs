using System.Threading.Tasks;
using crossql.Migrations;

namespace crossql.Extensions
{
    internal static class MigrationRunnerExtensions
    {
        public static async Task RunOrderedMigrationAsync(this IDbMigration migration, MigrationStep key, IDbProvider dbProvider)
        {
            if (!migration.Migration.ContainsKey(key))
                return;
            var database = new Database(dbProvider.DatabaseName, dbProvider.Dialect);
            migration.Migration[key].Invoke(database, dbProvider);
            var dbCommand = database.ToString();
            if (!string.IsNullOrWhiteSpace(dbCommand))
                await dbProvider.ExecuteNonQuery(dbCommand);
        }
    }
}