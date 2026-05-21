using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FinanceApp.Data.Models;
using FinanceApp.Services;
using FinanceApp.Services.Categories;

namespace FinanceApp.Views
{
    public partial class AddBudgetWindow : Window
    {
        private readonly BudgetService _budgetService = new BudgetService(new FinanceApp.Data.AppDbContext());
        private readonly CategoryService _categoryService = new CategoryService();

        public AddBudgetWindow()
        {
            InitializeComponent();
            Loaded += AddBudgetWindow_Loaded;
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private async void AddBudgetWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var allCategories = _categoryService.GetAll();
            var expenseCategories = allCategories.Where(c => c.Type == "Expense").ToList();

            cmbCategory.ItemsSource = expenseCategories;
            cmbCategory.DisplayMemberPath = "Name";

            cmbMonth.ItemsSource = Enumerable.Range(1, 12)
                .Select(m => new { Value = m, Name = new DateTime(2025, m, 1).ToString("MMMM") });
            cmbMonth.DisplayMemberPath = "Name";
            cmbMonth.SelectedIndex = DateTime.Now.Month - 1;

            cmbYear.ItemsSource = Enumerable.Range(DateTime.Now.Year - 1, 3);
            cmbYear.SelectedItem = DateTime.Now.Year;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (cmbCategory.SelectedItem is not Category selectedCategory)
            {
                MessageBox.Show("Выберите категорию", "Ошибка");
                return;
            }

            if (!decimal.TryParse(txtLimit.Text, out decimal limit) || limit <= 0)
            {
                MessageBox.Show("Введите корректный лимит", "Ошибка");
                return;
            }

            var budget = new Budget
            {
                UserId = App.CurrentUser?.Id ?? 0,
                CategoryId = selectedCategory.Id,
                PlannedAmount = limit,
                Year = (int)cmbYear.SelectedItem,
                Month = ((dynamic)cmbMonth.SelectedItem).Value
            };

            await _budgetService.SaveBudgetAsync(budget);

            MessageBox.Show("Бюджет успешно сохранён!", "Готово");
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
