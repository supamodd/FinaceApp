using System;
using System.Collections.Generic;
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
        private Budget? _editingBudget;

        // Конструктор для ДОБАВЛЕНИЯ
        public AddBudgetWindow() : this(null) { }

        // Конструктор для РЕДАКТИРОВАНИЯ
        public AddBudgetWindow(Budget? budgetToEdit)
        {
            InitializeComponent();
            _editingBudget = budgetToEdit;
            Loaded += AddBudgetWindow_Loaded;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
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

            // Месяцы
            var months = new List<string>();
            for (int m = 1; m <= 12; m++)
                months.Add(new DateTime(2025, m, 1).ToString("MMMM"));
            cmbMonth.ItemsSource = months;
            cmbMonth.SelectedIndex = DateTime.Now.Month - 1;

            // Год
            var years = Enumerable.Range(DateTime.Now.Year - 1, 3).ToList();
            cmbYear.ItemsSource = years;
            cmbYear.SelectedItem = DateTime.Now.Year;

            // Если редактирование — заполняем поля
            if (_editingBudget != null)
            {
                // Находим категорию в списке
                var category = expenseCategories.FirstOrDefault(c => c.Id == _editingBudget.CategoryId);
                if (category != null)
                    cmbCategory.SelectedItem = category;

                txtLimit.Text = _editingBudget.PlannedAmount.ToString();
                cmbMonth.SelectedIndex = _editingBudget.Month - 1;
                cmbYear.SelectedItem = _editingBudget.Year;
            }
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

            if (_editingBudget != null)
            {
                // Редактирование существующего
                _editingBudget.CategoryId = selectedCategory.Id;
                _editingBudget.PlannedAmount = limit;
                _editingBudget.Year = (int)cmbYear.SelectedItem;
                _editingBudget.Month = cmbMonth.SelectedIndex + 1;

                await _budgetService.SaveBudgetAsync(_editingBudget);
                MessageBox.Show("Бюджет обновлён!", "Готово");
            }
            else
            {
                // Создание нового
                var budget = new Budget
                {
                    UserId = App.CurrentUser?.Id ?? 0,
                    CategoryId = selectedCategory.Id,
                    PlannedAmount = limit,
                    Year = (int)cmbYear.SelectedItem,
                    Month = cmbMonth.SelectedIndex + 1
                };

                await _budgetService.SaveBudgetAsync(budget);
                MessageBox.Show("Бюджет успешно сохранён!", "Готово");
            }

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