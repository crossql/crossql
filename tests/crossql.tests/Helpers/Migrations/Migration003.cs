namespace crossql.tests.Helpers.Migrations
{
    public class Migration003 : CrossqlMigration
    {
        public Migration003() : base(3)
        {
            Migration[MigrationStep.Migrate] = (database, provider) =>
            {
                var autoTable = database.AddTable("Automobiles");
                autoTable.AddColumn("Vin", typeof(string),50).PrimaryKey().NotNullable();
                autoTable.AddColumn("VehicleType", typeof(string));
                autoTable.AddColumn("WheelCount", typeof(int));
            };
        }
    }
}