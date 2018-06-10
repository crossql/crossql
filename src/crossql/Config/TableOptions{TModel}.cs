using System;
using System.Linq.Expressions;
using crossql.Extensions;

namespace crossql.Config
{
    internal class TableOptions<TModel> : ITableOptions<TModel> where TModel : class, new()
    {
        /// <summary>
        ///     Tells the DbProvider what the PrimaryKey is for <see cref="TModel" />
        /// </summary>
        public ITableOptions<TModel> SetPrimaryKey(Expression<Func<TModel, object>> func)
        {
            if (!(func.Body is MemberExpression body))
            {
                var ubody = (UnaryExpression) func.Body;
                body = (MemberExpression)ubody.Operand;
            }
            var modelType = typeof(TModel);
            modelType.AddOrUpdatePrimaryKey(body.Member.Name);
            return this;
        }
    }
}