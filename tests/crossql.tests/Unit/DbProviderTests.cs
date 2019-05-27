using crossql.mysql;
using crossql.tests.Helpers.CustomTypes;
using FluentAssertions;
using NUnit.Framework;
using DbProvider = crossql.mssqlserver.DbProvider;

namespace crossql.tests.Unit
{
    [TestFixture]
    public class DbProviderTests
    {
        [Test]
        public void ShouldCreateDbProviderWithCustomDialect()
        {
            // setup
            var dbProvider = new DbProvider(new DbConnectionProvider("", ""),  "", cfg => { cfg.OverrideDialect(new SpecialDialect()); });
            
            // assert
            dbProvider.Dialect.DateTime.Should().Be("DATETIME");
        }
    }
}