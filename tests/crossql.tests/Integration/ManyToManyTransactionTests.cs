using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using crossql.tests.Helpers.Fixtures;
using crossql.tests.Helpers.Models;
using FluentAssertions;
using NUnit.Framework;

namespace crossql.tests.Integration
{
    [TestFixture]
    public class ManyToManyTransactionTests : IntegrationTestBase
    {
        [Test, TestCaseSource(nameof(DbProviders)), Ignore("won't compile under netstandard")]
        public async Task Should_Create_Records_With_ManyToMany_Relationships_Using_Transactions(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Setup
            var publisher = PublisherFixture.GetFirstPublisher();
            var firstBook = BookFixture.GetFirstBook(publisher);
            var secondBook = BookFixture.GetSecondBook(publisher);
            var expectedAuthor = AuthorFixture.GetFirstAuthor();
            expectedAuthor.AddBooks(firstBook, secondBook);

            await db.RunInTransaction(async trans =>
            {
                await trans.Create(publisher);
                await trans.Create(firstBook);
                await trans.Create(secondBook);
                await trans.Create(expectedAuthor);
            });

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
            //actualAuthor.Should().BeEquivalentTo(expectedAuthor, options => options.Excluding(a => a.PropertyPath.Contains("Publisher")));
        }

        [Test, TestCaseSource(nameof(DbProviders)), Ignore("won't compile under netstandard")]
        public async Task Should_Add_Records_To_ManyToMany_Relationship_Using_Transactions(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Setup
            var publisher = PublisherFixture.GetSecondPublisher();
            var firstBook = BookFixture.GetFirstBook(publisher);
            var secondBook = BookFixture.GetSecondBook(publisher);
            var expectedAuthor = AuthorFixture.GetSecondAuthor();
            expectedAuthor.AddBooks(firstBook, secondBook);

            // execute
            await db.RunInTransaction(async trans =>
            {
                await trans.CreateOrUpdate(publisher);
                await trans.CreateOrUpdate(firstBook);
                await trans.CreateOrUpdate(secondBook);
                await trans.CreateOrUpdate(expectedAuthor);
                await trans.CreateOrUpdate(expectedAuthor);
            });

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
            // actualAuthor.Should().BeEquivalentTo(expectedAuthor, options => options.Excluding(a => a.PropertyPath.Contains("Publisher")));
        }

        [Test, TestCaseSource(nameof(DbProviders)), Ignore("won't compile under netstandard")]
        public async Task Should_Remove_Records_From_ManyToMany_Relationship_Using_Transactions(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Setup
            var publisher = PublisherFixture.GetThirdPublisher();
            var firstBook = BookFixture.GetFirstBook(publisher);
            var secondBook = BookFixture.GetSecondBook(publisher);
            var expectedAuthor = AuthorFixture.GetThirdAuthor();
            expectedAuthor.RemoveBooks(firstBook);
            expectedAuthor.AddBooks(firstBook, secondBook);

            await db.RunInTransaction(async trans =>
            {
               await  trans.Create(publisher);
                await trans.Create(firstBook);
                await trans.Create(secondBook);
                await trans.Create(expectedAuthor);
                await trans.Update(expectedAuthor);
            });

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
            //actualAuthor.Should().BeEquivalentTo(expectedAuthor, options => options.Excluding(a => a.PropertyPath.Contains("Publisher")));
        }
    }
}