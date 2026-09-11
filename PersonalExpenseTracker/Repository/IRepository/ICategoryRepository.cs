using PersonalExpenseTracker.Models;

namespace PersonalExpenseTracker.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category category);
    }
}