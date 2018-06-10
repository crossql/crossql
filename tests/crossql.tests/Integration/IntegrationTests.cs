using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using crossql.tests.Helpers.Fixtures;
using crossql.tests.Helpers.Models;
using NUnit.Framework;
using crossql.Extensions;

namespace crossql.tests.Integration
{
    [TestFixture]
    public class IntegrationTests : IntegrationTestBase
    {
        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task ShouldDeleteAUserByEmailAddress(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Setup
            var expectedUser = new AuthorModel
            {
                Id = new Guid("57F98915-DBDF-41C7-9D24-F4BB1C0D9D0C"),
                FirstName = "Joe",
                LastName = "Blow",
                Email = "JoeBlow@microsoft.com",
            };
            await db.Create(expectedUser);

            // Execute
            await db.Delete<AuthorModel>(u => u.Email == expectedUser.Email);

            var actualUser = await db.Query<AuthorModel>().Where(u => u.Email == expectedUser.Email).ToListAsync();

            // Assert
            Assert.IsEmpty(actualUser);
        }

        [Test, TestCaseSource(nameof(DbProviders)),Ignore("Aarg, gotta figure this out")]
        public async Task ShouldDoCrud(IDbProvider db)
        {
            // don't run against SQLite because it's not seeded.
            var provider = db.GetType().ToString();
            if (provider == "FutureState.AppCore.Data.Sqlite.Windows.DbProvider") return;

            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Setup
            var expectedUser = new AuthorModel
            {
                Id = new Guid("381BC8C2-AF5D-40E1-81DD-620B4DCCEDBB"),
                FirstName = "SQL",
                LastName = "Admin",
                Email = "sa@microsoft.com",
            };

            // Execute Create
            await db.Create(expectedUser);
            var author = await db.Query<AuthorModel>().Where(u => u.Id == expectedUser.Id).FirstOrDefaultAsync();

            // Assert Create
            Assert.IsNotNull(author);
            Assert.IsNotNull(author.Id);
            Assert.AreNotEqual(Guid.Empty, author.Id);

            // Execute Find IEnumerable
            var actualUsers1 = await db.Query<AuthorModel>().Where(x => x.FirstName.Contains("jil")).SelectAsync();
            // this returns an IEnumerable

            // Assert Find IEnumerable
            Assert.True(actualUsers1.Any());

            // Execute Find List
            var actualUsers2 =
                await db.Query<AuthorModel>().Where(x => x.FirstName.Contains("jil") && x.LastName == "").ToListAsync();
            // ToList converts IEnumerable to a list

            // Assert Find List
            Assert.True(actualUsers2.Count > 0);

            // Execute Find List
            var stamp = DateTime.UtcNow.AddDays(-10);
            var actualUsers3 = await db.Query<AuthorModel>().Where(x => x.UpdatedDate >= stamp).ToListAsync();

            // Assert Find List
            Assert.True(actualUsers3.Count > 0);

            // Execute Read
            var actualUser = await db.Query<AuthorModel>().Where(x => x.Id == author.Id).FirstOrDefaultAsync();

            // Assert Read
            Assert.IsNotNull(actualUser);
            Assert.AreEqual(expectedUser.FirstName, actualUser.FirstName);
            Assert.AreEqual(expectedUser.LastName, actualUser.LastName);
            Assert.AreEqual(expectedUser.Email, actualUser.Email);

            // Execute Update
            actualUser.FirstName = "NewName";
            await db.Update(actualUser);
            actualUser = await db.Query<AuthorModel>().Where(x => x.Id == author.Id).FirstOrDefaultAsync();

            //// Assert Update
            Assert.IsNotNull(actualUser);
            Assert.AreEqual("NewName", actualUser.FirstName);
            Assert.AreEqual(expectedUser.LastName, actualUser.LastName);
            Assert.AreEqual(expectedUser.Email, actualUser.Email);

            // Execute Delete
            await db.Query<AuthorModel>().Where(u => u.Id == author.Id).DeleteAsync();
            actualUser = await db.Query<AuthorModel>().Where(x => x.Id == author.Id).FirstOrDefaultAsync();

            // Assert Delete
            Assert.IsNull(actualUser);
        }

        [Test, TestCaseSource(nameof(DbProviders)),Ignore("Aarg, gotta figure this out")]
        public async Task ShouldDoCrudWithGeese(IDbProvider db)
        {
            // don't run against SQLite because it's not seeded.
            var provider = db.GetType().ToString();
            if (provider == "FutureState.AppCore.Data.Sqlite.Windows.DbProvider") return;

            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Execute Create
            var firstGoose = new GooseEntity { Id = new Guid("43F4C249-E24C-41A7-9DED-73E3AE2C17BE"), Name = "My New Goose" };
            await db.Create(firstGoose);
            var goose = await db.Query<GooseEntity>().Where(u => u.Id == firstGoose.Id).FirstOrDefaultAsync();

            // Assert Create
            Assert.IsNotNull(goose);
            Assert.IsNotNull(goose.Id);
            Assert.AreEqual(firstGoose.Name, goose.Name);
            Assert.AreNotEqual(Guid.Empty, goose.Id);

            // Execute Find IEnumerable
            var actualGeese = await db.Query<GooseEntity>().Where(x => x.Name.Contains("irst")).SelectAsync();
            // this returns an IEnumerable

            // Assert Find IEnumerable
            Assert.True(actualGeese.Any());

            // Execute Find List
            var actualGeese2 = await db.Query<GooseEntity>().Where(x => x.Name.Contains("Goose")).ToListAsync();

            // Assert Find List
            Assert.True(actualGeese2.Count == 4);

            // Execute Update
            var gooseToUpdate = GooseFixture.GooseToUpdate;
            gooseToUpdate.Name = "Canada Goose";
            await db.Update(gooseToUpdate);
            var actualUpdatedGoose = await db.Query<GooseEntity>().Where(x => x.Id == gooseToUpdate.Id).FirstOrDefaultAsync();

            //// Assert Update
            Assert.IsNotNull(actualUpdatedGoose);
            Assert.AreEqual("Canada Goose", actualUpdatedGoose.Name);

            // Execute Delete
            var gooseToDelete = GooseFixture.GooseToDelete;
            await db.Query<GooseEntity>().Where(u => u.Id == gooseToDelete.Id).DeleteAsync();
            var actualDeletedGoose = db.Query<GooseEntity>().Where(x => x.Id == gooseToDelete.Id).FirstOrDefaultAsync();

            // Assert Delete
            Assert.IsNull(actualDeletedGoose);

            db.Query<GooseEntity>().Truncate();

            var emptyResults = await db.Query<GooseEntity>().SelectAsync();
            Assert.IsEmpty(emptyResults);
        }

        [Test, TestCaseSource(nameof(DbProviders)),Ignore("Aarg, gotta figure this out")]
        public async Task ShouldFindUserWithDateComparer(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Setup
            var stamp = DateTime.UtcNow.AddDays(-10);
            // Execute
            var actualUsers = await db.Query<AuthorModel>().Where(u => u.CreatedDate > stamp).ToListAsync();

            // Assert
            Assert.Greater(actualUsers.Count, 0);
        }

        [Test, TestCaseSource(nameof(DbProviders)),Ignore("Aarg, gotta figure this out")]
        public async Task ShouldFindUsersUsingSameFieldTwice(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Setup
            var expectedUsers = new List<AuthorModel>
            {
                new AuthorModel {FirstName = "Jill"},
                new AuthorModel {FirstName = "Bob"}
            };

            // Execute
            var actualUsers = await db.Query<AuthorModel>()
                .Where(u => u.FirstName.Contains("jil") || u.FirstName == "Bob")
                .ToListAsync();

            // Assert
            Assert.That(actualUsers.Any(s => s.FirstName == expectedUsers[0].FirstName));
            Assert.That(actualUsers.Any(s => s.FirstName == expectedUsers[1].FirstName));
            Assert.That(actualUsers.Count(s => s.FirstName == expectedUsers[0].FirstName), Is.EqualTo(1));
            Assert.That(actualUsers.Count(s => s.FirstName == expectedUsers[1].FirstName), Is.EqualTo(1));
        }

        [Test, TestCaseSource(nameof(DbProviders)),Ignore("Aarg, gotta figure this out")]
        public async Task ShouldCreateBookWithReferenceToItsAuthor(IDbProvider db)
        {
            var book = BookFixture.GetFirstBook();
            var id = new Guid("F6949DAD-810D-4724-A391-5C01DD9D66E8");
            book.Id = id;
            book.ISBN = 1919;

            // Execute
            await db.Create(book);
            var actualBook = await db.Query<BookModel>().Where(b => b.Id == id).FirstOrDefaultAsync();

            Assert.That(actualBook, Is.Not.Null);
            Assert.That(actualBook.Id, Is.EqualTo(id));
            Assert.That(actualBook.Authors.First().Id, Is.EqualTo(AuthorFixture.GetFirstAuthor().Id));
        }

        [Test, TestCaseSource(nameof(DbProviders)),Ignore("Aarg, gotta figure this out")]
        public async Task ShouldFindUsersWithLikeAndEqualsComparers(IDbProvider db)
        {
            // don't run against SQLite becase it's not seeded.
            if (db.GetType().ToString() == "FutureState.AppCore.Data.Sqlite.Windows.DbProvider") return;

            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Setup
            var expectedUsers = new List<AuthorModel> { new AuthorModel { FirstName = "Jill" } };

            // Execute
            var actualUsers =
                await db.Query<AuthorModel>().Where(u => u.FirstName.Contains("il") && u.LastName == "").ToListAsync();

            // Assert
            Assert.AreEqual(expectedUsers[0].FirstName, actualUsers[0].FirstName);
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task ShouldGetAllUsers(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Execute Query
            var actualUsers = await db.Query<AuthorModel>().ToListAsync();

            // Assert
            Assert.IsNotEmpty(actualUsers);
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task ShouldGetUserById(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            var expectedUser = new AuthorModel
            {
                Id = new Guid("5A7685A2-3EEC-442D-902F-D2022F28DD33"),
                FirstName = "test",
                LastName = "test",
                Email = "test@microsoft.com",
            };

            // Execute Create
            await db.Create(expectedUser);

            // Execute Query
            var actualUser = await db.Query<AuthorModel>().Where(u => u.Id == expectedUser.Id).FirstOrDefaultAsync();

            // Assert
            Assert.IsNotNull(actualUser);
            Assert.AreEqual(expectedUser.Id, actualUser.Id);
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public void ShouldSelectMaxCreatedDate(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));
            //const string expectedSelect = "SELECT MAX(CreatedDate) FROM Students";

            // Execute Query
            var actualSelect = db.Scalar<AuthorModel, DateTime>(s => s.CreatedDate).Max();

            // Assert
            Assert.IsNotNull(actualSelect);
            //Assert.AreEqual( expectedSelect, actualSelect );
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task ShouldReturnMinDateTimeFromEmptyTable(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            //setup
            var database = new Database(db.DatabaseName, db.Dialect);
            var fakeTable = database.AddTable("Fakes");
            fakeTable.AddColumn("TimeStamp", typeof(DateTime)).Nullable();
            await db.ExecuteNonQuery(database.ToString());

            // Execute Query
            var actualSelect = await db.Scalar<FakeModel, DateTime>(s => s.TimeStamp).MaxAsync();

            // Assert
            Assert.IsNotNull(actualSelect);
            //Assert.AreEqual( expectedSelect, actualSelect );
        }

        // todo: <Chase Florell: June 9, 2018> reimplement these tests
        //[Test, TestCaseSource(nameof(DbProviders))]
        //public async Task ShouldJoinToAnotherManyToManyTableAndBuildWhereClauseAndOrderByClause(IDbProvider db)
        //{
        //    // TODO: Implement This, data-dependent
        //    Trace.WriteLine(TraceObjectGraphInfo(db));

        //    // Setup
        //    var expectedStudents = AuthorFixture.FirstAuthor;
        //    var expectedCourse = PublisherFixture.FirstPublisher;

        //    // Execute
        //    var actualStudents = db.Query<AuthorModel>()
        //        .ManyToManyJoin<PublisherModel>()
        //        .Where((s, c) => c.Name == expectedCourse.Name)
        //        .OrderBy((s, c) => s.FirstName, OrderDirection.Descending)
        //        .SelectAsync()
        //        .ToList();

        //    // Assert
        //    Assert.AreEqual(1, actualStudents.Count());
        //    Assert.AreEqual(expectedStudents.Id, actualStudents[0].Id);
        //}
        
        //[Test, TestCaseSource(nameof(DbProviders))]
        //public async Task ShouldJoinToAnotherOneToManyTableAndBuildDefaultWhereClauseAndOrderByClause(IDbProvider db)
        //{
        //    // TODO: Implement This
        //    Trace.WriteLine(TraceObjectGraphInfo(db));

        //    // Setup
        //    var expectedStudents = new List<AuthorModel>
        //    {
        //        AuthorFixture.FirstAuthor
        //    };

        //    // Execute
        //    var actualStudents = db.Query<AuthorModel>()
        //        .LeftJoin<BookModel>()
        //        .Where((author, book) => book.Name == "FirstBookTitle")
        //        .OrderBy((author, book) => author.FirstName, OrderDirection.Descending)
        //        .SelectAsync()
        //        .ToList();

        //    // Assert
        //    Assert.AreEqual(1, actualStudents.Count());
        //    Assert.AreEqual(expectedStudents[0].Id, actualStudents[0].Id);
        //}

        //[Test, TestCaseSource(nameof(DbProviders))]
        //public async Task ShouldNotRunMigrations(IDbProvider db)
        //{
        //    // Setup
        //    var migrationRunner = new MigrationRunner(db);

        //    // Execute / Assert
        //    Assert.DoesNotThrow(() => migrationRunner.RunAll(SystemRole.Server, new List<IMigration> { new Migration001(), new Migration002() }));
        //    Assert.DoesNotThrow(() => migrationRunner.RunAll(SystemRole.Client, new List<IMigration> { new Migration001(), new Migration002() }));
        //}

        //[Test, TestCaseSource(nameof(DbProviders))]
        //public async Task ShouldJoinManyToManyTablesTogether(IDbProvider db)
        //{
        //    // don't run against SQLite becase it's not seeded.
        //    if (db.GetType().ToString() == "FutureState.AppCore.Data.Sqlite.Windows.DbProvider") return;

        //    Trace.WriteLine(TraceObjectGraphInfo(db));

        //    // Setup
        //    var mathCourseId = Migration001.BobsPublishingId;

        //    // Execute Query
        //    var actualUsers = db.Query<AuthorModel>()
        //        .ManyToManyJoin<PublisherModel>()
        //        .Where((s, b) => b.Id == mathCourseId)
        //        .SelectAsync()
        //        .ToList();

        //    // Assert
        //    Assert.IsNotEmpty(actualUsers);
        //}

        //[Test, TestCaseSource(nameof(DbProviders))]
        //public async Task ShouldAddToCollectionWithoutUniqueConstraintFailure(IDbProvider db)
        //{
        //    Trace.WriteLine(TraceObjectGraphInfo(db));

        //    // setup
        //    var studentToUpdate = AuthorFixture.AuthorToUpdate;
        //    studentToUpdate.Courses.Add(PublisherFixture.ThirdPublisher);

        //    // execute
        //    db.Update(studentToUpdate);

        //    var studentCourses = db.Query<PublisherModel>()
        //        .ManyToManyJoin<AuthorModel>()
        //        .Where((c, s) => c.IsDeleted == false && s.Id == studentToUpdate.Id)
        //        .SelectAsync()
        //        .ToList();

        //    // assert
        //    Assert.That(studentCourses.Count(), Is.EqualTo(2));
        //    Assert.That(studentCourses.Any(p => p.Id == PublisherFixture.ThirdPublisher.Id));
        //}

        //[Test, TestCaseSource(nameof(DbProviders))]
        //public async Task ShouldRemoveFromCollection(IDbProvider db)
        //{
        //    Trace.WriteLine(TraceObjectGraphInfo(db));

        //    // setup
        //    var studentToDelete = AuthorFixture.AuthorToDelete;
        //    studentToDelete.Courses.Remove(PublisherFixture.PublisherToDelete);

        //    // execute
        //    db.Update(studentToDelete);

        //    var studentCourses = db.Query<PublisherModel>()
        //        .ManyToManyJoin<AuthorModel>()
        //        .Where((c, s) => c.IsDeleted == false && s.Id == studentToDelete.Id)
        //        .SelectAsync()
        //        .ToList();

        //    // assert
        //    Assert.That(studentCourses.Count(), Is.EqualTo(0));
        //}

        //[Test, TestCaseSource(nameof(DbProviders))]
        //public async Task ShouldSayTheHighestbookNumberIsThree(IDbProvider db)
        //{
        //    Trace.WriteLine(TraceObjectGraphInfo(db));
        //    const int expectedNumber = 3;

        //    // Execute Query
        //    var actualNumber = db.Scalar<BookModel, int>(s => s.BookNumber).Max();

        //    // Assert
        //    Assert.That(actualNumber, Is.EqualTo(expectedNumber));
        //}
    }
}