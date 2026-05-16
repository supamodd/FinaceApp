using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceApp.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Путь к твоей SQLite базе (замени на свой, если нужно)
        optionsBuilder.UseSqlite("Data Source=finance.db");

        return new AppDbContext(optionsBuilder.Options);
    }
}