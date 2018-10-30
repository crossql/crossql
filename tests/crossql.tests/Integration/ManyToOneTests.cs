using System.Diagnostics;
using System.Threading.Tasks;
using crossql.Extensions;
using crossql.tests.Helpers.Fixtures;
using crossql.tests.Helpers.Models;
using FluentAssertions;
using NUnit.Framework;

namespace crossql.tests.Integration
{
    [TestFixture, Ignore("re-implement when joins are complete")]
    public class ManyToOneTests : IntegrationTestBase
    {
        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Create_Records_With_ManyToOne_Relationships(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Setup
            var publisher = PublisherFixture.GetFirstPublisher();
            var firstBook = BookFixture.GetFirstBook(publisher);
            var secondBook = BookFixture.GetSecondBook(publisher);

            await db.Create(publisher);
            await db.Create(firstBook);
            await db.Create(secondBook);

            // Execute
            var query = db.Query<BookModel>();
                                //.Join<PublisherModel>().On((b, p) => b.Publisher.Id == p.Id)
                                //.Where((b, p) => p.Id == publisher.Id);
            var actualBooks = await query.ToListAsync();
            
            // Assert
            actualBooks.Should().HaveCount(2);
            actualBooks[0].Publisher.Id.Should().Be(publisher.Id);
            //actualBooks[0].Publisher.Name.Should().Be(publisher.Name);
            actualBooks[1].Publisher.Id.Should().Be(publisher.Id);
            //actualBooks[1].Publisher.Name.Should().Be(publisher.Name);
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Create_Records_With_OneToMany_Relationships(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Setup
            var publisher = PublisherFixture.GetSecondPublisher();
            await db.Create(publisher);

            var thirdBook = BookFixture.GetThirdBook(publisher);
            var fourthBook = BookFixture.GetFourthBook(publisher);
            await db.Create(thirdBook);
            await db.Create(fourthBook);

            // Execute
            var query = db.Query<PublisherModel>();
                //.Join<BookModel>().On((p, b) => b.Publisher.Id == p.Id)
                //.Where((p, b) => b.Name == fourthBook.Name);
            var publishers = await query.ToListAsync();

            // Assert
            publishers.Should().HaveCount(1);
            publishers[0].Should().BeEquivalentTo(publisher);
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Query_ManyToOne_Records_With_Needing_To_Build_Join(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Setup
            var publisher = PublisherFixture.GetThirdPublisher();
            await db.Create(publisher);

            var thirdBook = BookFixture.GetThirdBook(publisher);
            var fourthBook = BookFixture.GetFourthBook(publisher);

            // Execute
            await db.Create(thirdBook);
            await db.Create(fourthBook);

            // Assert
            var query = db.Query<BookModel>()
                        .Where(b => b.Publisher.Id == publisher.Id);
            var actualBooks = await query.ToListAsync();

            actualBooks.Should().HaveCount(2);
            actualBooks[0].Publisher.Id.Should().Be(publisher.Id);
            actualBooks[1].Publisher.Id.Should().Be(publisher.Id);
        }
    }
}
