using System;
using System.Linq.Expressions;

namespace crossql.Config {
    public interface ITableOptions<TModel> where TModel : class, new() {
        /// <summary>
        ///     Tells the DbProvider what the PrimaryKey is for <see cref="TModel" />
        /// </summary>
        ITableOptions<TModel> SetPrimaryKey(Expression<Func<TModel, object>> func);
    }
}