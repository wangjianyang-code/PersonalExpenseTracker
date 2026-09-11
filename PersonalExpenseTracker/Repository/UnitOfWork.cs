using PersonalExpenseTracker.Data;
using PersonalExpenseTracker.Repository.IRepository;

namespace PersonalExpenseTracker.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public ICategoryRepository Category { get; private set; }

        public ITransactionRepository Transaction { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;

            Category = new CategoryRepository(_db);
            Transaction = new TransactionRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}