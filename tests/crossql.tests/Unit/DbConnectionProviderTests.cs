using System;
using System.IO;
using crossql.sqlite;
using FluentAssertions;
using NUnit.Framework;

namespace crossql.tests.Unit
{
    [TestFixture]
    public class DbConnectionProviderTests
    {
        private string _rootPath;

        [OneTimeSetUp]
        public void SetUp() => _rootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        [Test]
        public void ShouldConstructSqliteConnectionAndDisallowBrowsingTheDatabasePath()
        {
            // setup
            const string dbName = "foo.sqlite3";

            // execute
            var provider = new DbConnectionProvider(dbName, new SqliteSettings{BrowsableConnectionString = false});

            // assert
            provider.DatabasePath.Should().BeNullOrEmpty();
        }

        [Test]
        public void ShouldConstructSqliteConnectionWithAlternatePath()
        {
            // setup
            const string dbName = "foo.sqlite3";
            var expectedDatabasePath = $"{_rootPath}\\Documents\\{dbName}";

            // execute
            var provider = new DbConnectionProvider(dbName, fileName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), fileName));

            // assert
            provider.DatabasePath.Should().Be(expectedDatabasePath);
        }

        [Test]
        public void ShouldConstructSqliteConnectionWithDefaultPath()
        {
            // setup
            const string dbName = "foo";
            var expectedDatabasePath = $"{_rootPath}\\AppData\\Roaming\\{dbName}";

            // execute
            var provider = new DbConnectionProvider(dbName);

            // assert
            provider.DatabasePath.Should().Be(expectedDatabasePath);
        }
    }
}