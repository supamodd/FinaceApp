using Microsoft.EntityFrameworkCore;
using FinanceApp.Data.Models;

namespace FinanceApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<FinancialTransaction> Transactions { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Budget> Budgets { get; set; } = null!;
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; } = null!;
        public DbSet<Family> Families { get; set; }
        public DbSet<FamilyMember> FamilyMembers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "finance.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FinancialTransaction>()
                .HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId);

            modelBuilder.Entity<FinancialTransaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId);

            modelBuilder.Entity<FamilyMember>()
                .HasKey(fm => new { fm.FamilyId, fm.UserId });

            modelBuilder.Entity<FamilyMember>()
                .HasOne(fm => fm.Family)
                .WithMany(f => f.Members)
                .HasForeignKey(fm => fm.FamilyId);

            modelBuilder.Entity<FamilyMember>()
                .HasOne(fm => fm.User)
                .WithMany()
                .HasForeignKey(fm => fm.UserId);
        }
        public void EnsureCreated()
        {
            Database.EnsureCreated();
        }
    }
}