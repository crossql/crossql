using FutureState.AppCore.Data.Attributes;

namespace FutureState.AppCore.Data.Tests.Helpers.Models
{
    public class FooModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}