using System.Threading.Tasks;
using crossql.Migrations;

namespace crossql.tests.Helpers.Migrations
{
    public class Migration003 : DbMigration
    {
        public Migration003() : base(3) { }

        public override Task Migrate(Database database, IDbProvider provider)
        {
            var autoTable = database.AddTable("Automobiles");
            autoTable.AddColumn("Vin", typeof(string),50).PrimaryKey().NotNullable();
            autoTable.AddColumn("VehicleType", typeof(string));
            autoTable.AddColumn("WheelCount", typeof(int));        
            
            return Task.CompletedTask;
        }
    }
}