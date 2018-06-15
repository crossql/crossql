using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace crossql.Extensions
{
    public static class DbQueryExtensions
    {
        public static async Task<TResult> FirstAsync<TResult, TModel>(this IDbQuery<TModel> dbQuery, Func<IDataReader, IEnumerable<TResult>> mapperFunc)
            where TModel : class, new() => (await dbQuery.Select(mapperFunc).ConfigureAwait(false)).First();

        public static async Task<TModel> FirstAsync<TModel>(this IDbQuery<TModel> dbQuery) where TModel : class, new() =>
            (await dbQuery.Select().ConfigureAwait(false)).First();

        public static async Task<TResult> FirstOrDefaultAsync<TResult, TModel>(this IDbQuery<TModel> dbQuery, Func<IDataReader, IEnumerable<TResult>> mapperFunc)
            where TModel : class, new() => (await dbQuery.Select(mapperFunc).ConfigureAwait(false)).FirstOrDefault();

        public static async Task<TModel> FirstOrDefaultAsync<TModel>(this IDbQuery<TModel> dbQuery) where TModel : class, new() =>
            (await dbQuery.Select().ConfigureAwait(false)).FirstOrDefault();

        public static async Task<TModel> LastAsync<TModel>(this IDbQuery<TModel> dbQuery) where TModel : class, new() => (await dbQuery.Select().ConfigureAwait(false)).Last();

        public static async Task<TResult> LastAsync<TResult, TModel>(this IDbQuery<TModel> dbQuery, Func<IDataReader, IEnumerable<TResult>> mapperFunc)
            where TModel : class, new() => (await dbQuery.Select(mapperFunc).ConfigureAwait(false)).Last();

        public static async Task<TModel> LastOrDefaultAsync<TModel>(this IDbQuery<TModel> dbQuery) where TModel : class, new() =>
            (await dbQuery.Select().ConfigureAwait(false)).LastOrDefault();

        public static async Task<TResult> LastOrDefaultAsync<TResult, TModel>(this IDbQuery<TModel> dbQuery, Func<IDataReader, IEnumerable<TResult>> mapperFunc)
            where TModel : class, new() => (await dbQuery.Select(mapperFunc).ConfigureAwait(false)).LastOrDefault();

        public static async Task<TModel> SingleAsync<TModel>(this IDbQuery<TModel> dbQuery) where TModel : class, new() =>
            (await dbQuery.Select().ConfigureAwait(false)).Single();

        public static async Task<TResult> SingleAsync<TResult, TModel>(this IDbQuery<TModel> dbQuery, Func<IDataReader, IEnumerable<TResult>> mapperFunc)
            where TModel : class, new() => (await dbQuery.Select(mapperFunc).ConfigureAwait(false)).Single();

        public static async Task<TModel> SingleOrDefaultAsync<TModel>(this IDbQuery<TModel> dbQuery) where TModel : class, new() =>
            (await dbQuery.Select().ConfigureAwait(false)).SingleOrDefault();

        public static async Task<TResult> SingleOrDefaultAsync<TResult, TModel>(this IDbQuery<TModel> dbQuery, Func<IDataReader, IEnumerable<TResult>> mapperFunc)
            where TModel : class, new() => (await dbQuery.Select(mapperFunc).ConfigureAwait(false)).SingleOrDefault();

        public static async Task<IList<TModel>> ToListAsync<TModel>(this IDbQuery<TModel> dbQuery) where TModel : class, new() =>
            (await dbQuery.Select().ConfigureAwait(false)).ToList();

        public static async Task<IList<TResult>> ToListAsync<TResult, TModel>(this IDbQuery<TModel> dbQuery, Func<IDataReader, IEnumerable<TResult>> mapperFunc)
            where TModel : class, new() => (await dbQuery.Select(mapperFunc).ConfigureAwait(false)).ToList();
    }
}