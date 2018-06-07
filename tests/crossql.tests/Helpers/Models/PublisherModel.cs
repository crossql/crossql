using crossql.Attributes;

namespace crossql.tests.Helpers.Models
{
    public class PublisherModel : ModelBase
    {
        public string Name { get; set; }
        public string Description { get; set; }

        [OneToMany]
        public BookModel Books { get; set; }
    }
}