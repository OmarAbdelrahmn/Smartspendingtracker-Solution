using Microsoft.EntityFrameworkCore;
using Smartspendingtracker.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Smartspendingtracker
{
    /// <summary>
    /// Main database context for Smart Spending Tracker
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.NameEnglish);
                entity.HasIndex(e => e.NameArabic);
            });

            // Expense configuration
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasIndex(e => e.DateTime);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => new { e.DateTime, e.CategoryId });

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Expenses)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ExchangeRate configuration
            modelBuilder.Entity<ExchangeRate>(entity =>
            {
                entity.HasIndex(e => new { e.Year, e.Month, e.FromCurrency, e.ToCurrency })
                    .IsUnique();
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    NameEnglish = "Food",
                    NameArabic = "طعام",
                    Keywords = "أكل,مطعم,قهوة,فطار,غدا,عشا,طعام,food,restaurant,coffee,lunch,dinner,breakfast",
                    IconClass = "fa-utensils",
                    Color = "#FF6B6B"
                },
                new Category
                {
                    Id = 2,
                    NameEnglish = "Transportation",
                    NameArabic = "مواصلات",
                    Keywords = "مواصلات,بنزين,تاكسي,سيارة,transport,taxi,gas,car,fuel,uber",
                    IconClass = "fa-car",
                    Color = "#4ECDC4"
                },
                new Category
                {
                    Id = 3,
                    NameEnglish = "Bills",
                    NameArabic = "فواتير",
                    Keywords = "فواتير,كهرباء,ماء,نت,انترنت,bills,electricity,water,internet,utilities",
                    IconClass = "fa-file-invoice-dollar",
                    Color = "#95E1D3"
                },
                new Category
                {
                    Id = 4,
                    NameEnglish = "Rent",
                    NameArabic = "إيجار",
                    Keywords = "إيجار,ايجار,سكن,rent,housing",
                    IconClass = "fa-home",
                    Color = "#F38181"
                },
                new Category
                {
                    Id = 5,
                    NameEnglish = "Shopping",
                    NameArabic = "تسوق",
                    Keywords = "تسوق,ملابس,شراء,shopping,clothes,purchase,buy",
                    IconClass = "fa-shopping-bag",
                    Color = "#AA96DA"
                },
                new Category
                {
                    Id = 6,
                    NameEnglish = "Other",
                    NameArabic = "أخرى",
                    Keywords = "أخرى,متفرقات,other,miscellaneous,various",
                    IconClass = "fa-folder",
                    Color = "#6C757D"
                }
            );

            // Seed Exchange Rates for current month (February 2026)
            var now = DateTime.UtcNow.AddHours(3); // UTC+3
            modelBuilder.Entity<ExchangeRate>().HasData(
                new ExchangeRate
                {
                    Id = 1,
                    FromCurrency = Currency.SAR,
                    ToCurrency = Currency.EGP,
                    Rate = 13.5m, // 1 SAR = 13.5 EGP (example rate)
                    Year = now.Year,
                    Month = now.Month,
                    UpdatedAt = now
                }
            );
        }
    }
}