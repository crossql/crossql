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
        private string _userPath;

        [OneTimeSetUp]
        public void SetUp() => _userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        [Test]
        public void ShouldConstructSqliteConnectionWithAlternatePath()
        {
            // setup
            const string dbName = "foo";
            var expectedDatabasePath = $"{_userPath}\\Documents\\{dbName}";

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
            var expectedDatabasePath = $"{_userPath}\\AppData\\Roaming\\{dbName}";

            // execute
            var provider = new DbConnectionProvider(dbName);

            // assert
            provider.DatabasePath.Should().Be(expectedDatabasePath);
        }
    }
}