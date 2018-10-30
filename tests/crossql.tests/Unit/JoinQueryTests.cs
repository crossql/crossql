using System;
using crossql.tests.Helpers.Models;
using FluentAssertions;
using NUnit.Framework;

namespace crossql.tests.Unit
{
    [TestFixture]
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
                .Join<BookModel>(author => author.Books)
                .ToString();

            // assert
            expectedQuery.Should().Be(actualQuery);
        }

        [Test, TestCaseSource(nameof(Repositories))]
        public void ShouldGenerateSingleNestedJoinQuery(IDbProvider dbProvider)
        {
            // setup
            const string expectedQuery = @"SELECT [Books].* FROM [Books] 
INNER JOIN [Publishers] ON [Books].[Publisher] 
WHERE ( [Books].[IsDeleted] = @IsDeleted1 )";
            var authorId = new Guid("77F4D796-2485-455C-8477-A8A3FAFF873C");

            // execute
            var actualQuery = dbProvider.Query<BookModel>()
                .Join<PublisherModel>(book => book.Publisher)
                //.Join<BookModel>((book, publisher) => publisher.Books)
                //.Join<AuthorModel>((book, publisher) => book.Authors)
                //.Join<AuthorModel>((book, author) => author.Books)
                .Where(book => book.IsDeleted == false)
                .ToString();

            // assert
            expectedQuery.Should().Be(actualQuery);
        }
    }
}