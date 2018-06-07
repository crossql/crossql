using System;
using System.Threading.Tasks;
using crossql.tests.Helpers.Models;
using FluentAssertions;
using NUnit.Framework;

namespace crossql.tests.Integration
{
    [TestFixture]
    public class QueryHelperTests : IntegrationTestBase
    {
        [Test, TestCaseSource(nameof(DbProviders))]
        public void Should_Throw_When_First_Is_Empty(IDbProvider db)
        {
            // execute
            Func<Task<GooseModel>> method = async () => await db.Query<GooseModel>().FirstAsync();

            // assert
            method.Should().Throw<InvalidOperationException>();
        }
        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Return_Null_When_FirstOrDefault_Is_Empty(IDbProvider db)
        {
            // execute
            var result = await db.Query<GooseModel>().FirstOrDefaultAsync();

            // assert
            result.Should().BeNull();
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public void Should_Throw_When_Single_Is_Empty(IDbProvider db)
        {
            // execute
            Func<Task<GooseModel>> method = async () => await db.Query<GooseModel>().SingleAsync();

            // assert
            method.Should().Throw<InvalidOperationException>();
        }
        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Return_Null_When_SingleOrDefault_Is_Empty(IDbProvider db)
        {
            // execute
            var result = await db.Query<GooseModel>().SingleOrDefaultAsync();

            // assert
            result.Should().BeNull();
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public void Should_Throw_When_Last_Is_Empty(IDbProvider db)
        {
            // execute
            Func<Task<GooseModel>> method = async () => await db.Query<GooseModel>().LastAsync();

            // assert
            method.Should().Throw<InvalidOperationException>();
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Return_Null_When_LastOrDefault_Is_Empty(IDbProvider db)
        {
            // execute
            var result = await db.Query<GooseModel>().LastOrDefaultAsync();

            // assert
            result.Should().BeNull();
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Return_Zero_Records_When_ToList_Is_Empty(IDbProvider db)
        {
            // execute
            var result = await db.Query<GooseModel>().ToListAsync();

            // assert
            result.Count.Should().Be(0);
        }
    }
}