using System.Diagnostics;
using System.Threading.Tasks;
using crossql.tests.Helpers.Fixtures;
using crossql.tests.Helpers.Models;
using FluentAssertions;
using NUnit.Framework;

namespace crossql.tests.Integration
{
    [TestFixture]
    public class SimpleCrudTests : IntegrationTestBase
    {
        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Do_Crud_On_Simple_Model_Object(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // Create
            var expectedAuthor = AuthorFixture.GetFirstAuthor();
            await db.Create(expectedAuthor);

            // Assert Create
            var actualAuthor = await db.Query<AuthorModel>().Where(a => a.Email == expectedAuthor.Email).SingleOrDefaultAsync();
            actualAuthor.Should().NotBeNull();
            actualAuthor.Should().BeEquivalentTo(expectedAuthor);

            // Update
            expectedAuthor.FirstName = "Bob";
            expectedAuthor.LastName = "Jones";
            await db.Update(expectedAuthor);

            // Assert Update
            actualAuthor = await db.Query<AuthorModel>().Where(a => a.Email == expectedAuthor.Email).SingleOrDefaultAsync();
            actualAuthor.Should().NotBeNull();
            actualAuthor.Should().BeEquivalentTo(expectedAuthor);

            // Delete
            await db.Delete<AuthorModel>(x => x.Id == expectedAuthor.Id);

            // Assert Delete
            actualAuthor = await db.Query<AuthorModel>().Where(a => a.Email == expectedAuthor.Email).SingleOrDefaultAsync();
            actualAuthor.Should().BeNull();
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Do_Crud_On_Simple_Model_Object_With_Different_Primary_Key(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // create
            var car = AutomobileFixture.GetCar();
            await db.Create(car);

            // assert create
            var actualCar = await db.Query<AutomobileModel>().Where(c => c.Vin == car.Vin).SingleAsync();
            actualCar.Should().NotBeNull();
            actualCar.Should().BeEquivalentTo(car);

            // update
            car.WheelCount = 6;
            car.VehicleType = "Argo";
            await db.Update(car);

            // assert update
            actualCar = await db.Query<AutomobileModel>().Where(c => c.Vin == car.Vin).SingleAsync();
            actualCar.Should().NotBeNull();
            actualCar.Should().BeEquivalentTo(actualCar);

            // delete
            await db.Delete<AutomobileModel>(c => c.Vin == car.Vin);

            // assert delete
            actualCar = await db.Query<AutomobileModel>().Where(c => c.Vin == car.Vin).SingleOrDefaultAsync();
            actualCar.Should().BeNull();
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Create_Or_Update_With_Different_Primary_Key(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // create
            var motorcycle = AutomobileFixture.GetMotorcycle();
            await db.CreateOrUpdate(motorcycle);

            // assert create
            var actualMotorcycle = await db.Query<AutomobileModel>().Where(c => c.Vin == motorcycle.Vin).SingleAsync();
            actualMotorcycle.Should().NotBeNull();
            actualMotorcycle.Should().BeEquivalentTo(motorcycle);

            // upsert
            motorcycle.VehicleType = "Scooter";
            await db.CreateOrUpdate(motorcycle);

            // assert update
            actualMotorcycle = await db.Query<AutomobileModel>().Where(c => c.Vin == motorcycle.Vin).SingleAsync();
            actualMotorcycle.Should().NotBeNull();
            actualMotorcycle.Should().BeEquivalentTo(actualMotorcycle);
            actualMotorcycle.VehicleType.Should().Be(motorcycle.VehicleType);

            // delete
            await db.Delete<AutomobileModel>(c => c.Vin == motorcycle.Vin);

            // assert delete
            actualMotorcycle = await db.Query<AutomobileModel>().Where(c => c.Vin == motorcycle.Vin).SingleOrDefaultAsync();
            actualMotorcycle.Should().BeNull();
        }
    }
}