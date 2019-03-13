using System;
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
        [Test]
        [TestCaseSource(nameof(DbProviders))]
        public async Task Should_Create_Records_With_AutoIncrement(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // setup
            var foos = new List<FooModel>();
            for (var i = 1; i < 21; i++) foos.Add(new FooModel {Name = $"Name-{i}"});

            // execute
            await db.RunInTransaction(async trans => { 
                foreach (var foo in foos) 
                    await trans.Create(foo).ConfigureAwait(false);
            });
            var actualFoos = await db.Query<FooModel>().ToListAsync();
            actualFoos.Count.Should().Be(20);
        }

        [Test]
        [TestCaseSource(nameof(DbProviders))]
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

        [Test]
        [TestCaseSource(nameof(DbProviders))]
        public async Task Should_Perform_Faster_When_Run_In_Transaction(IDbProvider db)
        {
            var firstLoop = 1000;
            var secondLoop = 2000;
            const int secondIndexStart = 1010;
            var expectedVehicleCount = 1980;
            
            // make sqlite work harder because we also do an in memory test
            if (db is sqlite.DbProvider)
            {
                firstLoop = 10000;
                secondLoop = 20000;
                expectedVehicleCount = 19990;
            };
            
            Trace.WriteLine(TraceObjectGraphInfo(db));
        
            // setup
            var carWatch = new Stopwatch();
            var bikeWatch = new Stopwatch();
        
            // transaction test
            var car = AutomobileFixture.GetCar();
            carWatch.Start();
            await db.RunInTransaction(async trans =>
            {
                for (var i = 10; i < firstLoop; i++) 
                {
                    car.Vin = i.ToString();
                    await trans.CreateOrUpdate(car);
                }
            });
            carWatch.Stop();
        
            // non transaction test
            var motorcycle = AutomobileFixture.GetMotorcycle();
            bikeWatch.Start();
            for (var i = secondIndexStart; i < secondLoop; i++) 
            {
                motorcycle.Vin = i.ToString();
                await db.CreateOrUpdate(motorcycle);
            }
        
            bikeWatch.Stop();
            carWatch.ElapsedTicks.Should().BeLessThan(bikeWatch.ElapsedTicks);
        
            // assert record count
            var vehicleCount = (await db.Query<Automobile>().ToListAsync()).Count;
            vehicleCount.Should().Be(expectedVehicleCount);
        
            Trace.WriteLine($"Non Transactionable: {bikeWatch.Elapsed:hh\\:mm\\:ss} \t(Ticks {bikeWatch.ElapsedTicks})");
            Trace.WriteLine($"Transactionable: {carWatch.Elapsed:hh\\:mm\\:ss} \t\t(Ticks {carWatch.ElapsedTicks})");
        }

        [Test]
        [TestCaseSource(nameof(DbProviders))]
        public async Task ShouldRollBackFailedTransaction(IDbProvider db)
        {
            Trace.WriteLine(TraceObjectGraphInfo(db));

            // setup
            var truck = AutomobileFixture.GetTruck();
            Func<Task> action = async () => await db.RunInTransaction(async transaction =>
            {
                await transaction.Create(truck); // even though this doesn't fail, it should be rolled back in the transaction.
                await transaction.ExecuteNonQuery("INVALID SQL QUERY");
            });

            // assert throws
            action.Should().Throw<Exception>();

            // execute
            var actualTruck = await db.Query<Automobile>().Where(a => a.Vin == truck.Vin).FirstOrDefaultAsync();

            // assert null
            actualTruck.Should().BeNull();
        }
    }
}