using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using crossql.Extensions;
using crossql.tests.Helpers.Fixtures;
using crossql.tests.Helpers.Models;
using FluentAssertions;
using NUnit.Framework;

namespace crossql.tests.Integration
{
    [TestFixture]
    public class ManyToManyTests : IntegrationTestBase
    {
        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Create_Records_With_ManyToMany_Relationships(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Setup
            var publisher = PublisherFixture.GetFirstPublisher();
            await db.Create(publisher);

            var firstBook = BookFixture.GetFirstBook(publisher);
            var secondBook = BookFixture.GetSecondBook(publisher);
            await db.Create(firstBook);
            await db.Create(secondBook);

            var expectedAuthor = AuthorFixture.GetFirstAuthor();
            expectedAuthor.AddBooks(firstBook, secondBook);

            // Execute
            await db.Create(expectedAuthor);

            // Assert
            var actualAuthor = await db.Query<AuthorModel>()
                                 .Where(a => a.Email == expectedAuthor.Email)
                                 .SingleAsync();

            actualAuthor.Should().NotBeNull();

            var moreBooks = await db.Query<BookModel>()
                .ManyToManyJoin<AuthorModel>()
                .Where((b, a) => a.Id == actualAuthor.Id)
                .SelectAsync();
            actualAuthor.AddBooks(moreBooks.ToArray());

            // Excluding publisher info because only its ID is included in the hydration.
            actualAuthor.Should().BeEquivalentTo(expectedAuthor, options => options.Excluding(a => a.SelectedMemberPath.Contains("Publisher")));
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Add_Records_To_ManyToMany_Relationship(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Setup
            var publisher = PublisherFixture.GetSecondPublisher();
            await db.Create(publisher);

            var firstBook = BookFixture.GetFirstBook(publisher);
            var secondBook = BookFixture.GetSecondBook(publisher);
            await db.Create(firstBook);
            await db.Create(secondBook);

            var expectedAuthor = AuthorFixture.GetSecondAuthor();
            await db.Create(expectedAuthor);

            // Execute
            expectedAuthor.AddBooks(firstBook, secondBook);
            await db.Update(expectedAuthor);

            // Assert
            var actualAuthor = await db.Query<AuthorModel>()
                                 .Where(a => a.Email == expectedAuthor.Email)
                                 .SingleAsync();

            actualAuthor.Should().NotBeNull();

            var moreBooks = await db.Query<BookModel>()
                .ManyToManyJoin<AuthorModel>()
                .Where((b,a)=> a.Id == actualAuthor.Id)
                .SelectAsync();
            actualAuthor.AddBooks(moreBooks.ToArray());

            // Excluding publisher info because only its ID is included in the hydration.
            actualAuthor.Should().BeEquivalentTo(expectedAuthor, options => options.Excluding(a => a.SelectedMemberPath.Contains("Publisher")));
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Remove_Records_From_ManyToMany_Relationship(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Setup
            var publisher = PublisherFixture.GetThirdPublisher();
            await db.Create(publisher);

            var firstBook = BookFixture.GetFirstBook(publisher);
            var secondBook = BookFixture.GetSecondBook(publisher);
            await db.Create(firstBook);
            await db.Create(secondBook);

            var expectedAuthor = AuthorFixture.GetThirdAuthor();
            expectedAuthor.AddBooks(firstBook, secondBook);
            await db.Create(expectedAuthor);

            // Execute
            expectedAuthor.RemoveBooks(firstBook);
            await db.Update(expectedAuthor);

            // Assert
            var actualAuthor = await db.Query<AuthorModel>()
                                 .Where(a => a.Email == expectedAuthor.Email)
                                 .SingleAsync();

            actualAuthor.Should().NotBeNull();

            var moreBooks = await db.Query<BookModel>()
                .ManyToManyJoin<AuthorModel>()
                .Where((b, a) => a.Id == actualAuthor.Id)
                .SelectAsync();
            actualAuthor.AddBooks(moreBooks.ToArray());

            // Excluding publisher info because only its ID is included in the hydration.
            actualAuthor.Should().BeEquivalentTo(expectedAuthor, options => options.Excluding(a => a.SelectedMemberPath.Contains("Publisher")));
        }
    }
}