using crossql.Attributes;

namespace crossql.tests.Helpers.Models
{
    public class Automobile
    {
        [PrimaryKey]
        public string Vin { get; set; }
        public int WheelCount { get; set; }
        public string VehicleType { get; set; }
    }
}