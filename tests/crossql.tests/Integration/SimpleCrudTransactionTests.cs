using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using crossql.Extensions;
using crossql.tests.Helpers.Fixtures;
using crossql.tests.Helpers.Models;
using FluentAssertions;
using NUnit.Framework;

namespace crossql.tests.Integration
{
    public class SimpleCrudTransactionTests : IntegrationTestBase
    {
        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Do_CUD_In_Transactions(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // create
            var motorcycle = AutomobileFixture.GetMotorcycle();
            var car = AutomobileFixture.GetCar();
            await db.RunInTransaction(async transaction =>
            {
                await transaction.Create(motorcycle); // create only
                await transaction.CreateOrUpdate(car); // create or update
            });

            // assert create
            var actualMotorcycle = await db.Query<Automobile>().Where(a => a.Vin == motorcycle.Vin).SingleOrDefaultAsync();
            var actualCar = await db.Query<Automobile>().Where(a => a.Vin == car.Vin).SingleOrDefaultAsync();
            actualMotorcycle.Should().NotBeNull();
            actualCar.Should().NotBeNull();

            // update
            motorcycle.VehicleType = "scooter";
            car.VehicleType = "truck";
            await db.RunInTransaction(async transaction =>
            {
                await transaction.CreateOrUpdate(motorcycle); // create or update
                await transaction.Update(car); // update only
            });

            // assert update
            actualMotorcycle = await db.Query<Automobile>().Where(a => a.Vin == motorcycle.Vin).SingleOrDefaultAsync();
            actualCar = await db.Query<Automobile>().Where(a => a.Vin == car.Vin).SingleOrDefaultAsync();
            actualMotorcycle.Should().NotBeNull();
            actualCar.Should().NotBeNull();
            actualMotorcycle.VehicleType.Should().Be(motorcycle.VehicleType);
            actualCar.VehicleType.Should().Be(car.VehicleType);

            // delete
            await db.RunInTransaction(async transaction =>
            {
                await transaction.Delete<Automobile>(a => a.Vin == motorcycle.Vin);
                await transaction.Delete<Automobile>(a => a.Vin == car.Vin);
            });

            // assert delete
            actualMotorcycle = await db.Query<Automobile>().Where(a => a.Vin == motorcycle.Vin).SingleOrDefaultAsync();
            actualCar = await db.Query<Automobile>().Where(a => a.Vin == car.Vin).SingleOrDefaultAsync();
            actualMotorcycle.Should().BeNull();
            actualCar.Should().BeNull();
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Perform_Faster_When_Run_In_Transaction(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // setup
            var carWatch = new Stopwatch();
            var bikeWatch = new Stopwatch();

            // transaction test
            var car = AutomobileFixture.GetCar();
            carWatch.Start();
            await db.RunInTransaction(async trans =>
            {
                for (var i = 10; i < 1000; i++) // 990 records
                {
                    car.Vin = i.ToString();
                    await trans.CreateOrUpdate(car);
                }
            });
            carWatch.Stop();

            // non transaction test
            var motorcycle = AutomobileFixture.GetMotorcycle();
            bikeWatch.Start();
            for (var i = 1010; i < 2000; i++) // 990 records
            {
                motorcycle.Vin = i.ToString();
                await db.CreateOrUpdate(motorcycle);
            }
            bikeWatch.Stop();
            carWatch.ElapsedTicks.Should().BeLessThan(bikeWatch.ElapsedTicks);

            // assert record count
            var vehicleCount = (await db.Query<Automobile>().ToListAsync()).Count;
            vehicleCount.Should().Be(1980);

            Trace.WriteLine($"Non Transactionable: {bikeWatch.Elapsed:hh\\:mm\\:ss} \t(Ticks {bikeWatch.ElapsedTicks})");
            Trace.WriteLine($"Transactionable: {carWatch.Elapsed:hh\\:mm\\:ss} \t\t(Ticks {carWatch.ElapsedTicks})");
        }

        [Test, TestCaseSource(nameof(DbProviders))]
        public async Task Should_Create_Records_With_AutoIncrement(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // setup
            var foos = new List<FooModel>();
            for (var i = 1; i < 21; i++)
            {
                foos.Add(new FooModel { Name = $"Name-{i}" });
            }

            // execute
            foreach (var foo in foos)
            {
                await db.Create(foo);
            }
            var actualFoos = await db.Query<FooModel>().ToListAsync();
            actualFoos.Count.Should().Be(20);
        }
    }
}