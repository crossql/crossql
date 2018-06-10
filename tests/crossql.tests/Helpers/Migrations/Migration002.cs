using System;
using crossql.Migrations;

namespace crossql.tests.Helpers.Migrations
{
    public class Migration002 : DbMigration
    {
        public Migration002():base(2)
        {
            Migration[MigrationStep.Migrate] = (database, dbProvider) =>
            {
                var gooseTable = database.UpdateTable("Geese");
                gooseTable.AddColumn("BirthDate", typeof (DateTime)).Nullable();
                gooseTable.AddColumn("IsDead", typeof (bool)).NotNullable(false);
                //database.AddIndex("Geese", "BirthDate");
            };
        }
    }
}