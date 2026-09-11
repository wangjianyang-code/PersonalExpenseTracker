using Microsoft.EntityFrameworkCore;
using PersonalExpenseTracker.Models;

namespace PersonalExpenseTracker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Category> Categories { get; set; }
    }
}