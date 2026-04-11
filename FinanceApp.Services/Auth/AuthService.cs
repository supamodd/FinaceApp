using FinanceApp.Data;
using FinanceApp.Data.Models;
using BCrypt.Net;
using FinanceApp.Services.Categories;

namespace FinanceApp.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context = new AppDbContext();

        public User? Login(string login, string password)
        {
            _context.EnsureCreated();   // на всякий случай

            var user = _context.Users.FirstOrDefault(u => u.Login == login);
            if (user == null) return null;

            bool passwordCorrect = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            return passwordCorrect ? user : null;
        }

        public User? Register(string login, string password, string fullName)
        {
            _context.EnsureCreated();

            if (_context.Users.Any(u => u.Login == login))
                return null;

            var user = new User
            {
                Login = login,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                FullName = fullName
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Создаём стандартные категории
            var categoryService = new CategoryService();
            categoryService.EnsureDefaultCategoriesExist();

            return user;
        }
    }
}