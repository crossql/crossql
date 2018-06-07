using System;
using crossql.tests.Helpers.Models;

namespace crossql.tests.Helpers.Fixtures
{
    public class GooseFixture : FixtureBase
    {
        public static GooseModel GetFirstGoose()
        {
            return new GooseModel
            {
                Id = Guid.NewGuid(),
                Name = "FirstGoose",
            };
        }

        public static GooseModel GetGooseToUpdate()
        {
            return new GooseModel
            {
                Id = Guid.NewGuid(),
                Name = "GooseToUpdate",
            };
        }

        public static GooseModel GetGooseToDelete()
        {
            return new GooseModel
            {
                Id = Guid.NewGuid(),
                Name = "GooseToDelete",
            };
        }
    }
}