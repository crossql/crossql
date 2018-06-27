using System.Collections.Generic;
using System.Threading.Tasks;

namespace crossql.Extensions {
    public static class TransactionableExtensions
    {
        /// <summary>
        ///     Create a new record based on a Model
        /// </summary>
        /// <typeparam name="TModel">Model Type</typeparam>
        /// <param name="transactionable"></param>
        /// <param name="model">Model Object</param>
        /// <returns>The uniqueidentifier (Guid) of the newly created record.</returns>
        public static Task Create<TModel>(this ITransactionable transactionable, TModel model) where TModel : class, new() => transactionable.Create(model, new AutoDbMapper<TModel>());

        /// <summary>
        ///     Update the record if it doesn't exist, otherwise create a new one.
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="transactionable"></param>
        /// <param name="model">Model Object to create or update</param>
        public static Task CreateOrUpdate<TModel>(this ITransactionable transactionable,TModel model) where TModel : class, new() => transactionable.CreateOrUpdate(model, new AutoDbMapper<TModel>());
        
        /// <summary>
        ///     Execute a Non Query (Create, Update, Delete)
        /// </summary>
        /// <param name="transactionable"></param>
        /// <param name="commandText">command text to execute</param>
        /// <returns><see cref="Task"/></returns>
        public static Task ExecuteNonQuery(this ITransactionable transactionable, string commandText) => transactionable.ExecuteNonQuery(commandText, new Dictionary<string, object>());

        /// <summary>
        ///     Update the Database Record of a specified model.
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <param name="transactionable"></param>
        /// <param name="model">Model Object to update</param>
        public static Task Update<TModel>(this ITransactionable transactionable, TModel model) where TModel : class, new() => transactionable.Update(model, new AutoDbMapper<TModel>());
    }
}