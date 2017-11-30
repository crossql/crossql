namespace FutureState.AppCore.Data.Tests.Helpers.Migrations
{
    public class Migration004 : AppCoreMigration
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