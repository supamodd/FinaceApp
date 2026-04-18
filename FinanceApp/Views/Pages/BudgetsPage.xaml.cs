using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FinanceApp.Data.Models;
using FinanceApp.Services;
using FinanceApp.Services.Transactions;

namespace FinanceApp.Views.Pages
{
    public partial class BudgetsPage : UserControl
    {
        private readonly BudgetService _budgetService = new BudgetService(new FinanceApp.Data.AppDbContext());
        private readonly TransactionService _transactionService = new TransactionService();

        public BudgetsPage()
        {
            InitializeComponent();
            this.Loaded += BudgetsPage_Loaded;
        }

        private async void BudgetsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBudgetsAsync();
        }

        private async Task LoadBudgetsAsync()
        {
            if (App.CurrentUser == null) return;

            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;

            var budgets = await _budgetService.GetBudgetsForMonthAsync(App.CurrentUser.Id, year, month);

            var budgetViewModels = new List<BudgetViewModel>();

            foreach (var budget in budgets)
            {
                decimal spent = await GetSpentInCategoryAsync(budget.CategoryId, year, month);

                decimal percent = budget.PlannedAmount > 0
                    ? (spent / budget.PlannedAmount) * 100
                    : 0;

                string status = percent > 100 ? "Превышен ❌" :
                               percent > 80 ? "Почти превышен ⚠️" : "В норме ✅";

                budgetViewModels.Add(new BudgetViewModel
                {
                    Budget = budget,
                    Spent = spent,
                    ProgressPercent = (double)Math.Min(percent, 100),
                    SpentInfo = $"{spent:N0} / {budget.PlannedAmount:N0} ₽",
                    Status = status
                });
            }

            dgBudgets.ItemsSource = budgetViewModels
                .OrderByDescending(b => b.ProgressPercent)
                .ThenBy(b => b.Budget.Category.Name)
                .ToList();
        }

        private async Task<decimal> GetSpentInCategoryAsync(int categoryId, int year, int month)
        {
            if (App.CurrentUser == null) return 0;

            var transactions = _transactionService.GetAll(App.CurrentUser.Id);

            return transactions
                .Where(t => t.CategoryId == categoryId &&
                            t.Type == "Expense" &&
                            t.Date.Year == year &&
                            t.Date.Month == month)
                .Sum(t => t.Amount);
        }

        private void AddBudget_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddBudgetWindow();
            if (window.ShowDialog() == true)
            {
                LoadBudgetsAsync();         // обновляем список
            }
        }
    }

    public class BudgetViewModel
    {
        public Budget Budget { get; set; } = null!;
        public decimal Spent { get; set; }
        public double ProgressPercent { get; set; }
        public string SpentInfo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}