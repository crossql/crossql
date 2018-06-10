using crossql.Migrations;

namespace crossql.tests.Helpers.Migrations
{
    public class Migration004 : DbMigration
    {
        public Migration004() : base(4)
        {
            Migration[MigrationStep.Migrate] = (database, provider) =>
            {
                var fooTable = database.AddTable("Foos");
                fooTable.AddColumn("Id", typeof(int)).PrimaryKey().NotNullable().AutoIncrement(1, 1);
                fooTable.AddColumn("Name", typeof(string), 256);
            };
        }
    }
}