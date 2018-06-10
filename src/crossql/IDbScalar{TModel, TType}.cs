using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace crossql
{
    public interface IDbScalar<TModel, TType> where TModel : class, new()
    {
        IDbScalar<TModel, TType> Where ( Expression<Func<TModel, object>> whereExpression );
        Task<TType> MaxAsync();
        Task<TType> MinAsync();
        Task<TType> SumAsync();

        string ToStringMax();
        string ToStringMin();
        string ToStringSum();
    }
}