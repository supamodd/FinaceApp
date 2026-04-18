using FinanceApp.Data;
using FinanceApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Services
{
    public class BudgetService
    {
        private readonly AppDbContext _context;   // ← укажи имя своего контекста (AppDbContext или FinanceAppContext)

        public BudgetService(AppDbContext context)
        {
            _context = context;
        }

        // Получить все бюджеты пользователя за определённый месяц
        public async Task<List<Budget>> GetBudgetsForMonthAsync(int userId, int year, int month)
        {
            return await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId && b.Year == year && b.Month == month)
                .ToListAsync();
        }

        // Получить бюджет по категории и месяцу (или null, если нет)
        public async Task<Budget?> GetBudgetForCategoryAsync(int userId, int categoryId, int year, int month)
        {
            return await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.UserId == userId
                                       && b.CategoryId == categoryId
                                       && b.Year == year
                                       && b.Month == month);
        }

        // Создать или обновить бюджет
        public async Task SaveBudgetAsync(Budget budget)
        {
            var existing = await GetBudgetForCategoryAsync(
                budget.UserId,
                budget.CategoryId,
                budget.Year,
                budget.Month);

            if (existing != null)
            {
                existing.PlannedAmount = budget.PlannedAmount;
                _context.Budgets.Update(existing);
            }
            else
            {
                _context.Budgets.Add(budget);
            }

            await _context.SaveChangesAsync();
        }

        // Удалить бюджет
        public async Task DeleteBudgetAsync(int budgetId)
        {
            var budget = await _context.Budgets.FindAsync(budgetId);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();
            }
        }
    }
}