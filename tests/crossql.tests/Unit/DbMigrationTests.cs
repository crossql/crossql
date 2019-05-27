using System;
using crossql.sqlite;
using crossql.mssqlserver;
using crossql.tests.Helpers.CustomTypes;
using NUnit.Framework;

namespace crossql.tests.Unit
{
    [TestFixture]
    public class DbMigrationTests
    {
        private readonly string _nl = Environment.NewLine;
        [Test]
        public void ShouldAddADefaultValueToAField()
        {
            // Setup
            string expectedDDL = "CREATE TABLE [Test] (" + _nl +
                                 "[Id] int DEFAULT(7)," + _nl +
                                 "[Foo] nvarchar(max) );";

            var dialect = new SqlServerDialect();
            var database = new Database("MyDatabase", dialect);

            var testTable = database.AddTable("Test");
            testTable.AddColumn("Id", typeof (int)).Default(7);
            testTable.AddColumn("Foo", typeof (string));

            // Execute
            var actualDDL = database.ToString();

            // Assert
            Assert.AreEqual(expectedDDL, actualDDL);
        }

        [Test]
        public void ShouldUpdateAnExistingTableAndAddANewColumn ()
        {
            // Setup
            string expectedDDL = "ALTER TABLE [Test] ADD " + _nl +
                                 "[Name] nvarchar(100) NULL;" + _nl +
                                 "ALTER TABLE [Test] ADD " + _nl +
                                 "[Foo] int DEFAULT(1);" + _nl +
                                 "CREATE INDEX [IX_Test_Name_Foo] ON [Test] (Name,Foo);";

            var dialect = new SqlServerDialect();
            var database = new Database( "MyDatabase", dialect );

            var testTable = database.UpdateTable( "Test" );
            testTable.AddColumn( "Name", typeof( string ), 100 ).Nullable();
            testTable.AddColumn( "Foo", typeof( int ) ).Default(1);
            database.AddIndex("Test", "Name", "Foo");

            // Execute
            var actualDDL = database.ToString();

            // Assert
            Assert.AreEqual( expectedDDL, actualDDL );
        }

        [Test]
        public void ShouldBuildProperDDLForANewSqlServerDatabase()
        {
            // Setup
            string expectedDDL = "CREATE TABLE [Teachers] (" + _nl +
                                 "[Id] uniqueidentifier NOT NULL NONCLUSTERED PRIMARY KEY," + _nl +
                                 "[TeacherName] nvarchar(100) NOT NULL);" + _nl +
                                 "CREATE TABLE [Courses] (" + _nl +
                                 "[Id] uniqueidentifier NOT NULL NONCLUSTERED PRIMARY KEY," + _nl +
                                 "[CourseName] nvarchar(100) NOT NULL," + _nl +
                                 "[CourseDescription] nvarchar(max) ," + _nl +
                                 "[CourseTeacher] uniqueidentifier NOT NULL CONSTRAINT FK_Courses_CourseTeacher FOREIGN KEY (CourseTeacher) REFERENCES Teachers (Id) ON DELETE NO ACTION ON UPDATE NO ACTION," + _nl +
                                 "[IsAvailable] bit DEFAULT(0) NOT NULL);";

            var dialect = new SqlServerDialect();
            var database = new Database("MyDatabase", dialect);

            var teacher = database.AddTable("Teachers");
            teacher.AddColumn("Id", typeof (Guid)).PrimaryKey().NonClustered().NotNullable();
            teacher.AddColumn("TeacherName", typeof (String), 100).NotNullable();

            var course = database.AddTable("Courses");
            course.AddColumn("Id", typeof (Guid)).PrimaryKey().NonClustered().NotNullable();
            course.AddColumn("CourseName", typeof (String), 100).NotNullable();
            course.AddColumn("CourseDescription", typeof (String));
            course.AddColumn("CourseTeacher", typeof (Guid)).ForeignKey("Teachers", "Id").NotNullable();
            course.AddColumn("IsAvailable", typeof (bool)).NotNullable(false);

            // Execute
            var actualDDL = database.ToString();

            // Assert
            Assert.AreEqual(expectedDDL, actualDDL);
        }

        [Test]
        public void ShouldBuildProperDDLForANewSqliteDatabase()
        {
            // Setup
            string expectedDDL = "CREATE TABLE [Teachers] (" + _nl +
                                 "[Id] blob NOT NULL PRIMARY KEY," + _nl +
                                 "[TeacherName] text NOT NULL);" + _nl +
                                 "CREATE TABLE [Courses] (" + _nl +
                                 "[Id] blob NOT NULL PRIMARY KEY," + _nl +
                                 "[CourseName] text NOT NULL," + _nl +
                                 "[CourseDescription] text ," + _nl +
                                 "[CourseTeacher] blob NOT NULL REFERENCES Teachers (Id) ON DELETE NO ACTION ON UPDATE NO ACTION," + _nl +
                                 "[IsAvailable] integer DEFAULT(0) NOT NULL);";

            var dialect = new SqliteDialect();
            var database = new Database("MyDatabase", dialect);

            var teacher = database.AddTable("Teachers");
            teacher.AddColumn("Id", typeof (Guid)).PrimaryKey().NotNullable();
            teacher.AddColumn("TeacherName", typeof (String), 100).NotNullable();

            var course = database.AddTable("Courses");
            course.AddColumn("Id", typeof (Guid)).PrimaryKey().NotNullable();
            course.AddColumn("CourseName", typeof (String), 100).NotNullable();
            course.AddColumn("CourseDescription", typeof (String));
            course.AddColumn("CourseTeacher", typeof (Guid)).ForeignKey("Teachers", "Id").NotNullable();
            course.AddColumn("IsAvailable", typeof (bool)).NotNullable(false);

            // Execute
            var actualDDL = database.ToString();

            // Assert
            Assert.AreEqual(expectedDDL, actualDDL);
        }

        [Test]
        public void ShouldBuildTableWithCompositeKey()
        {
            // Setup
            string expectedDDL = "CREATE TABLE [Roles_Permissions] (" + _nl +
                                 "[RoleId] uniqueidentifier CONSTRAINT FK_Roles_Permissions_RoleId FOREIGN KEY (RoleId) REFERENCES Courses (Id) ON DELETE NO ACTION ON UPDATE NO ACTION," + _nl +
                                 "[PermissionId] uniqueidentifier CONSTRAINT FK_Roles_Permissions_PermissionId FOREIGN KEY (PermissionId) REFERENCES Permissions (Id) ON DELETE NO ACTION ON UPDATE NO ACTION," + _nl +
                                 "CONSTRAINT PK_Roles_Permissions_Composite PRIMARY KEY NONCLUSTERED (RoleId, PermissionId));";

            var dialect = new SqlServerDialect();
            var database = new Database("MyDatabase", dialect);

            var rolesPermissionsTable = database.AddTable("Roles_Permissions").CompositeKey("RoleId", "PermissionId");
            rolesPermissionsTable.AddColumn("RoleId", typeof (Guid)).ForeignKey("Courses", "Id");
            rolesPermissionsTable.AddColumn("PermissionId", typeof (Guid)).ForeignKey("Permissions", "Id");

            // Execute
            var actualDDL = database.ToString();

            // Assert
            Assert.AreEqual(expectedDDL, actualDDL);
        }

        [Test]
        public void ShouldBuildTableWithCompositeUnique()
        {
            // setup
            string expectedDDL = "CREATE TABLE [Roles_Permissions] (" + _nl +
                                 "[RoleId] uniqueidentifier CONSTRAINT FK_Roles_Permissions_RoleId FOREIGN KEY (RoleId) REFERENCES Courses (Id) ON DELETE NO ACTION ON UPDATE NO ACTION," + _nl +
                                 "[PermissionId] uniqueidentifier CONSTRAINT FK_Roles_Permissions_PermissionId FOREIGN KEY (PermissionId) REFERENCES Permissions (Id) ON DELETE NO ACTION ON UPDATE NO ACTION," + _nl +
                                 "CONSTRAINT PK_RoleId_PermissionId_Composite UNIQUE NONCLUSTERED (RoleId, PermissionId));";


            var dialect = new SqlServerDialect();
            var database = new Database("MyDatabase", dialect);

            var rolesPermissionsTable = database.AddTable("Roles_Permissions").CompositeUnique("RoleId", "PermissionId");
            rolesPermissionsTable.AddColumn("RoleId", typeof (Guid)).ForeignKey("Courses", "Id");
            rolesPermissionsTable.AddColumn("PermissionId", typeof (Guid)).ForeignKey("Permissions", "Id");

            // Execute
            var actualDDL = database.ToString();

            // Assert
            Assert.AreEqual(expectedDDL, actualDDL);
        }

        [Test]
        public void ShouldCreateATableWithANotNullInt()
        {
            // Setup
            string expectedDDL = "CREATE TABLE [Test] (" + _nl +
                                 "[Id] int NOT NULL," + _nl +
                                 "[Foo] nvarchar(max) );";

            var dialect = new SqlServerDialect();
            var database = new Database("MyDatabase", dialect);

            var testTable = database.AddTable("Test");
            testTable.AddColumn("Id", typeof (int)).NotNullable();
            testTable.AddColumn("Foo", typeof (string));

            // Execute
            var actualDDL = database.ToString();

            // Assert
            Assert.AreEqual(expectedDDL, actualDDL);
        }

        [Test]
        public void ShouldCreateATableWithALatLongColumn()
        {
            // Setup
            string expectedDDL = "CREATE TABLE [Test] (" + _nl +
                                 "[Latitude] double(9, 6) );";

            var dialect = new SqlServerDialect();
            var customDialect = new CustomDialect();
            var database = new Database("MyDatabase", dialect);

            var testTable = database.AddTable("Test");
            testTable.AddColumn("Latitude", typeof (LatLong)).AsCustomType(customDialect.LatLong);

            // Execute
            var actualDDL = database.ToString();

            // Assert
            Assert.AreEqual(expectedDDL, actualDDL);
        }

        [Test]
        public void ShouldCreateATableWithAnOverriddenDataTypeViaCustomDialect()
        {
            // Setup
            string expectedDDL = "CREATE TABLE [Test] (" + _nl +
                                 "[MobileCreatedAt] DATETIME );";

            var dialect = new SqliteDialect();
            var customDialect = new SpecialDialect();
            var database = new Database("MyDatabase", dialect);

            var testTable = database.AddTable("Test");
            testTable.AddColumn("MobileCreatedAt", typeof (DateTime)).AsCustomType(customDialect.DateTime);;

            // Execute
            var actualDDL = database.ToString();

            // Assert
            Assert.AreEqual(expectedDDL, actualDDL);
        }

        [Test]
        public void ShouldCreateATableWithAnOverriddenDataTypeViaCustomDialectRedux()
        {
            // Setup
            string expectedDDL = "CREATE TABLE [Test] (" + _nl +
                                 "[MobileCreatedAt] DATETIME );";

            var dialect = new SpecialDialect();
            var database = new Database("MyDatabase", dialect);

            var testTable = database.AddTable("Test");
            testTable.AddColumn("MobileCreatedAt", typeof (DateTime));

            // Execute
            var actualDDL = database.ToString();

            // Assert
            Assert.AreEqual(expectedDDL, actualDDL);
        }

        [Test]
        public void ShouldCreateATableWithAnOverriddenDataTypeViaCustomDialectHardCoded()
        {
            // Setup
            string expectedDDL = "CREATE TABLE [Test] (" + _nl +
                                 "[MobileCreatedAt] DATETIME );";

            var dialect = new SqliteDialect();
            var database = new Database("MyDatabase", dialect);

            var testTable = database.AddTable("Test");
            testTable.AddColumn("MobileCreatedAt", typeof (DateTime)).AsCustomType("DATETIME");;

            // Execute
            var actualDDL = database.ToString();

            // Assert
            Assert.AreEqual(expectedDDL, actualDDL);
        }
    }
}