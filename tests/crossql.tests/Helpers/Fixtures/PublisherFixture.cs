using System;
using crossql.tests.Helpers.Models;

namespace crossql.tests.Helpers.Fixtures
{
    public class PublisherFixture : FixtureBase
    {
        public static PublisherModel GetFirstPublisher()
        {
            return UpdateBaseFields(new PublisherModel
            {
                Id = Guid.NewGuid(),
                Name = "First",
                Description = "First publisher description",
            });
        }

        public static PublisherModel GetSecondPublisher()
        {
            return UpdateBaseFields(new PublisherModel
            {
                Id = Guid.NewGuid(),
                Name = "Second",
                Description = "Second publisher description",
            });
        }

        public static PublisherModel GetThirdPublisher()
        {
            return UpdateBaseFields(new PublisherModel
            {
                Id = Guid.NewGuid(),
                Name = "Third",
                Description = "Third publisher description"
            });
        }

        public static PublisherModel GetFourthPublisher()
        {
            return UpdateBaseFields(new PublisherModel
            {
                Id = Guid.NewGuid(),
                Name = "Fourth",
                Description = "Fourth publisher description"
            });
        }

        public static PublisherModel GetPublisherToUpdate()
        {
            return UpdateBaseFields(new PublisherModel
            {
                Id = Guid.NewGuid(),
                Name = "Updatable",
                Description = "Updatable publisher description"
            });
        }

        public static PublisherModel GetPublisherToDelete()
        {
            return UpdateBaseFields(new PublisherModel
            {
                Id = Guid.NewGuid(),
                Name = "Deletable",
                Description = "Deletable publisher description"
            });
        }
    }
}