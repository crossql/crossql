using System;
using crossql.tests.Helpers.Models;

namespace crossql.tests.Helpers.Fixtures
{
    public class GooseFixture : FixtureBase
    {
        public static GooseEntity GetFirstGoose()
        {
            return new GooseEntity
            {
                Id = Guid.NewGuid(),
                Name = "FirstGoose",
            };
        }

        public static GooseEntity GetGooseToUpdate()
        {
            return new GooseEntity
            {
                Id = Guid.NewGuid(),
                Name = "GooseToUpdate",
            };
        }

        public static GooseEntity GetGooseToDelete()
        {
            return new GooseEntity
            {
                Id = Guid.NewGuid(),
                Name = "GooseToDelete",
            };
        }

        public static GooseEntity GooseToUpdate { get; set; }
        public static GooseEntity GooseToDelete { get; set; }
    }
}