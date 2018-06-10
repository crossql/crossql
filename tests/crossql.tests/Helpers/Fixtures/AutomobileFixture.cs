using crossql.tests.Helpers.Models;

namespace crossql.tests.Helpers.Fixtures
{
    public class AutomobileFixture : FixtureBase
    {
        public static Automobile GetCar()
        {
            return new Automobile
            {
                Vin = "FEAB8F4C-72EA-4206-8473-15852772204B",
                WheelCount = 4,
                VehicleType = "Car"
            };
        }
        public static Automobile GetMotorcycle()
        {
            return new Automobile
            {
                Vin = "85AD5224-074C-4EB6-A778-A8C5ED2E24EC",
                WheelCount = 2,
                VehicleType = "Motorcycle"
            };
        }
    }
}