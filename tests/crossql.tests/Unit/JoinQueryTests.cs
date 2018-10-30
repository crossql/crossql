using System;
using crossql.tests.Helpers.Models;
using FluentAssertions;
using NUnit.Framework;

namespace crossql.tests.Unit
{
    [TestFixture, Ignore("re-implement when join is complete")]
    public class JoinQueryTests : DbQueryTestBase
    {
        [Test, TestCaseSource(nameof(Repositories))]
        public void ShouldGenerateBasicJoinQuery(IDbProvider dbProvider)
        {
            // setup
            const string expectedQuery = @"SELECT [Authors].* FROM [Authors] 
INNER JOIN [Authors_Books] ON [Authors_Books].[AuthorId] = [Authors].[Id]";
            
            // execute
            var actualQuery = dbProvider.Query<AuthorModel>()
                                        .Join<BookModel>(author => author.Books).ToString();

            // assert
            expectedQuery.Should().Be(actualQuery);
        }
        
        [Test, TestCaseSource(nameof(Repositories))]
        public void ShouldGenerateSingleNestedJoinQuery(IDbProvider dbProvider)
        {
            // setup
            const string expectedQuery = "SELECT [Authors].* FROM [Authors] LEFT OUTER JOIN [Books] ON [Authors].[Books] WHERE ( [Books].[IsDeleted] = @IsDeleted1 )";
            var authorId = new Guid("77F4D796-2485-455C-8477-A8A3FAFF873C");
            
            // execute
            var actualQuery = dbProvider.Query<AuthorModel>()
                .Join<BookModel>(author => author.Books)
                .ToString();

            // assert
            expectedQuery.Should().Be(actualQuery);
        }
    }
}