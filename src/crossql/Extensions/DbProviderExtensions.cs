using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace crossql.Extensions
{
    public static class DbProviderExtensions
    {
        public static Task<TResult> ExecuteReader<TResult>(this IDbProvider dbProvider,string commandText, Func<IDataReader, TResult> readerMapper) => dbProvider.ExecuteReader(commandText, new Dictionary<string, object>(), readerMapper);
        public static Task<TKey> ExecuteScalar<TKey>(this IDbProvider dbProvider, string commandText) => dbProvider.ExecuteScalar<TKey>(commandText, new Dictionary<string, object>());
    }
}