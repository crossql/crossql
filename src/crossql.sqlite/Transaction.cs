using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace crossql.sqlite
{
    public class Transaction:TransactionBase
    {
        public Transaction(IDbConnection connection, IDbTransaction transaction, IDbCommand command, IDialect dialect) : base(connection, transaction, command, dialect) { }
        public override Task ExecuteNonQuery(string commandText, IDictionary<string, object> parameters)
        {
            throw new System.NotImplementedException();
        }
    }
}