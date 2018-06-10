using System.Collections.Generic;
using System.Threading.Tasks;

namespace crossql.sqlite
{
    public class Transactionable:TransactionableBase
    {
        public Transactionable(IDbConnectionProvider provider, IDialect dialect) : base( provider, dialect) { }
        public override Task CreateOrUpdate<TModel>(TModel model, IDbMapper<TModel> dbMapper)
        {
            throw new System.NotImplementedException();
        }

        public override Task ExecuteNonQuery(string commandText, IDictionary<string, object> parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}