using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using crossql.mssqlserver;
using crossql.Migrations;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Version = crossql.Models.Version;

namespace crossql.tests.Unit
{
    [TestFixture]
    public class MigrationRunnerTests
    {
        private Mock<Database> _database;
        private Mock<IDbProvider> _provider;
        private Mock<IDbQuery<Version>> _query;

        [SetUp]
        public void SetUp()
        {
            _database = new Mock<Database>("foo", new SqlServerDialect());
            _provider = new Mock<IDbProvider>();
            _query = new Mock<IDbQuery<Version>>();
            
            _provider.SetupGet(p => p.Dialect).Returns(new SqlServerDialect());
            _provider.SetupGet(p => p.DatabaseName).Returns("foo");
            _provider.Setup(x => x.Query<Version>()).Returns(_query.Object);
            _query.Setup(x => x.Where(It.IsAny<Expression<Func<Version, bool>>>())).Returns(_query.Object);
            _query.Setup(x => x.OrderBy(It.IsAny<Expression<Func<Version, object>>>(), It.IsAny<OrderDirection>())).Returns(_query.Object);
            _query.Setup(x => x.Select()).ReturnsAsync(new List<Version>());
        }

        [Test]
        public async Task ShouldRunAStandardMigration()
        {
            // setup
            var migration = new Mock<IDbMigration>();
            migration.Setup(x => x.Migrate(_database.Object, _provider.Object));
            migration.SetupGet(p => p.MigrationVersion).Returns(1);
            
            var runner = new MigrationRunner( _provider.Object);
            
            // execute
            await runner.Run(SystemRole.Server, migration.Object);

            // assert
            migration.Verify( m => m.Migrate(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Once);
        }

        [Test]
        public void ShouldTerminateWhenMigrationFails()
        {
            // setup
            var thrown = new Exception("BOOM!");
            var migration1 = new Mock<IDbMigration>();
            var migration2 = new Mock<IDbMigration>();
            
            migration1.Setup(x => x.SetupMigration(_database.Object, _provider.Object));
            migration1.SetupGet(p => p.MigrationVersion).Returns(1);
            migration1.Setup(x => x.Migrate(It.IsAny<Database>(), It.IsAny<IDbProvider>())).Throws(thrown);
            migration1.Setup(x => x.MigrationFailed(It.IsAny<Database>(), It.IsAny<IDbProvider>(),
                It.IsAny<MigrationStep>(), It.IsAny<Exception>()));
            
            migration2.Setup(x => x.SetupMigration(_database.Object, _provider.Object));
            migration2.SetupGet(p => p.MigrationVersion).Returns(2);
            migration2.Setup(x => x.Migrate(It.IsAny<Database>(), It.IsAny<IDbProvider>()));
            
            var runner = new MigrationRunner( _provider.Object);

            // exectue
            Func<Task> action = () => runner.RunAll(SystemRole.Server, new []{migration1.Object, migration2.Object});

            // assert
            action.Should().Throw<Exception>();
            migration1.Verify(x => x.MigrationFailed(It.IsAny<Database>(), It.IsAny<IDbProvider>(), It.Is<MigrationStep>(s => s == MigrationStep.Migrate),It.Is<Exception>(ex => ex == thrown)));
            migration2.Verify(x => x.Migrate(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Never);
        }
        
        [Test]
        public async Task ShouldRunAllMigrationStepsForClientWithoutThrowing()
        {
            // setup
            var migration = new Mock<IDbMigration>();
            migration.SetupGet(p => p.MigrationVersion).Returns(1);
            migration.Setup(x => x.SetupMigration(_database.Object, _provider.Object));
            migration.Setup(x => x.ClientSetupMigration(_database.Object, _provider.Object));
            migration.Setup(x => x.ServerSetupMigration(_database.Object, _provider.Object));
            
            migration.Setup(x => x.Migrate(_database.Object, _provider.Object));
            migration.Setup(x => x.ClientMigrate(_database.Object, _provider.Object));
            migration.Setup(x => x.ServerMigrate(_database.Object, _provider.Object));
            
            migration.Setup(x => x.FinishMigration(_database.Object, _provider.Object));
            migration.Setup(x => x.ClientFinishMigration(_database.Object, _provider.Object));
            migration.Setup(x => x.ServerFinishMigration(_database.Object, _provider.Object));
            
            var runner = new MigrationRunner( _provider.Object);
            
            // execute
            await runner.Run(SystemRole.Client, migration.Object);

            // assert
            migration.Verify( m => m.SetupMigration(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Once);
            migration.Verify( m => m.ClientSetupMigration(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Once);
            migration.Verify( m => m.ServerSetupMigration(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Never);
            
            migration.Verify( m => m.Migrate(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Once);
            migration.Verify( m => m.ClientMigrate(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Once);
            migration.Verify( m => m.ServerMigrate(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Never);
            
            migration.Verify( m => m.FinishMigration(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Once);
            migration.Verify( m => m.ClientFinishMigration(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Once);
            migration.Verify( m => m.ServerFinishMigration(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Never);
        }
        
        [Test]
        public async Task ShouldRunAllMigrationStepsForServerWithoutThrowing()
        {
            // setup
            var migration = new Mock<IDbMigration>();
            migration.SetupGet(p => p.MigrationVersion).Returns(1);
            migration.Setup(x => x.SetupMigration(_database.Object, _provider.Object));
            migration.Setup(x => x.ClientSetupMigration(_database.Object, _provider.Object));
            migration.Setup(x => x.ServerSetupMigration(_database.Object, _provider.Object));
            
            migration.Setup(x => x.Migrate(_database.Object, _provider.Object));
            migration.Setup(x => x.ClientMigrate(_database.Object, _provider.Object));
            migration.Setup(x => x.ServerMigrate(_database.Object, _provider.Object));
            
            migration.Setup(x => x.FinishMigration(_database.Object, _provider.Object));
            migration.Setup(x => x.ClientFinishMigration(_database.Object, _provider.Object));
            migration.Setup(x => x.ServerFinishMigration(_database.Object, _provider.Object));
            
            var runner = new MigrationRunner( _provider.Object);
            
            // execute
            await runner.Run(SystemRole.Server, migration.Object);

            // assert
            migration.Verify( m => m.SetupMigration(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Once);
            migration.Verify( m => m.ClientSetupMigration(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Never);
            migration.Verify( m => m.ServerSetupMigration(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Once);
            
            migration.Verify( m => m.Migrate(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Once);
            migration.Verify( m => m.ClientMigrate(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Never);
            migration.Verify( m => m.ServerMigrate(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Once);
            
            migration.Verify( m => m.FinishMigration(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Once);
            migration.Verify( m => m.ClientFinishMigration(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Never);
            migration.Verify( m => m.ServerFinishMigration(It.IsAny<Database>(), It.IsAny<IDbProvider>()), Times.Once);
        }
    }
}