using System.Collections.Generic;
using System.Data;

namespace crossql
{
    public interface IDbMapper<TMapTo> where TMapTo : class, new()
    {
        IList<string> FieldNames { get; }
        IDictionary<string, object> BuildDbParametersFrom(TMapTo model);
        TMapTo BuildFrom(IDataReader dbReader);
        IEnumerable<TMapTo> BuildListFrom(IDataReader reader);
    }
}