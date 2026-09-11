using PersonalExpenseTracker.Models;

namespace PersonalExpenseTracker.Repository.IRepository
{
    public interface ITransactionRepository : IRepository<Transaction>
    {
        void Update(Transaction transaction);
    }
}