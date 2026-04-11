using FinanceApp.Data;
using FinanceApp.Data.Models;

namespace FinanceApp.Services.Categories
{
    public class CategoryService
    {
        private readonly AppDbContext _context = new AppDbContext();

        public List<Category> GetAll()
        {
            _context.EnsureCreated();
            return _context.Categories.ToList();
        }

        public void Add(Category category)
        {
            _context.EnsureCreated();
            _context.Categories.Add(category);
            _context.SaveChanges();
        }

        // Надёжное создание дефолтных категорий
        public void EnsureDefaultCategoriesExist()
        {
            _context.EnsureCreated();

            if (_context.Categories.Any())
                return;

            var defaultCategories = new List<Category>
            {
                new Category { Name = "Еда",           Type = "Expense", Color = "#FF5722" },
                new Category { Name = "Транспорт",     Type = "Expense", Color = "#3F51B5" },
                new Category { Name = "Зарплата",      Type = "Income",  Color = "#4CAF50" },
                new Category { Name = "Развлечения",   Type = "Expense", Color = "#9C27B0" },
                new Category { Name = "Коммуналка",    Type = "Expense", Color = "#F44336" },
                new Category { Name = "Здоровье",      Type = "Expense", Color = "#00BCD4" },
                new Category { Name = "Подарки",       Type = "Expense", Color = "#E91E63" },
                new Category { Name = "Другое",        Type = "Expense", Color = "#607D8B" }
            };

            _context.Categories.AddRange(defaultCategories);
            _context.SaveChanges();
        }
    }
}