using Microsoft.EntityFrameworkCore;

namespace SpendingTracker.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Budget> Budgets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed default categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Food & Dining", Color = "#e74c3c", Icon = "fa-utensils", IsExpense = true },
                new Category { Id = 2, Name = "Transportation", Color = "#3498db", Icon = "fa-car", IsExpense = true },
                new Category { Id = 3, Name = "Shopping", Color = "#9b59b6", Icon = "fa-shopping-bag", IsExpense = true },
                new Category { Id = 4, Name = "Entertainment", Color = "#f39c12", Icon = "fa-film", IsExpense = true },
                new Category { Id = 5, Name = "Bills & Utilities", Color = "#e67e22", Icon = "fa-bolt", IsExpense = true },
                new Category { Id = 6, Name = "Healthcare", Color = "#1abc9c", Icon = "fa-heartbeat", IsExpense = true },
                new Category { Id = 7, Name = "Salary", Color = "#27ae60", Icon = "fa-money-bill-wave", IsExpense = false },
                new Category { Id = 8, Name = "Freelance", Color = "#16a085", Icon = "fa-laptop-code", IsExpense = false }
            );
        }
    }
}