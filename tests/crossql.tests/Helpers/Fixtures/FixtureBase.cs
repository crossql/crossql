using System;
using crossql.tests.Helpers.Models;

namespace crossql.tests.Helpers.Fixtures
{
    public abstract class FixtureBase
    {
        private static readonly DateTime Date = new DateTime(2014, 12, 1);

        public static TModel UpdateBaseFields<TModel>(TModel model) where TModel : ModelBase
        {
            model.CreatedDate = Date;
            model.UpdatedDate = Date;
            model.IsDeleted = false;

            return model;
        }
    }
}