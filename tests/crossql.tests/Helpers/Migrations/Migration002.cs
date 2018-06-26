using System;
using System.Threading.Tasks;
using crossql.Migrations;

namespace crossql.tests.Helpers.Migrations
{
    public class Migration002 : DbMigration
    {
        public Migration002():base(2) { }

        public override Task Migrate(Database database, IDbProvider provider)
        {
            var gooseTable = database.UpdateTable("Geese");
            gooseTable.AddColumn("BirthDate", typeof (DateTime)).Nullable();
            gooseTable.AddColumn("IsDead", typeof (bool)).NotNullable(false);
            //database.AddIndex("Geese", "BirthDate");
            
            return Task.CompletedTask;
        }
    }
}