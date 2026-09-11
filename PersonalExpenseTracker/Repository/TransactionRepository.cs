using PersonalExpenseTracker.Data;
using PersonalExpenseTracker.Models;
using PersonalExpenseTracker.Repository.IRepository;

namespace PersonalExpenseTracker.Repository
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        private readonly ApplicationDbContext _db;

        public TransactionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Transaction transaction)
        {
            _db.Transactions.Update(transaction);
        }
    }
}