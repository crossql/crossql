using crossql.Attributes;

namespace crossql.tests.Helpers.Models
{
    public class FooModel
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}