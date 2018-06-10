using System.Threading.Tasks;

namespace crossql {
    public interface ITransactionRunner
    {
        Task Initialize(bool useTransaction);
        void Commit();
        void Rollback();
    }
}