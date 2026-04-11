using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FinanceApp.Data.Models;
using FinanceApp.Services.Categories;
using FinanceApp.Services.Transactions;

namespace FinanceApp.Views
{
    public partial class AddTransactionWindow : Window
    {
        private readonly TransactionService _transactionService = new TransactionService();
        private readonly CategoryService _categoryService = new CategoryService();
        private FinancialTransaction? _transactionToEdit;

        public AddTransactionWindow(FinancialTransaction? transaction = null)
        {
            InitializeComponent();

            cmbType.SelectionChanged += CmbType_SelectionChanged;

            // Принудительно создаём стандартные категории при открытии окна
            _categoryService.EnsureDefaultCategoriesExist();

            LoadCategories();

            if (_transactionToEdit != null)
            {
                Title = "Редактирование операции";
                dpDate.SelectedDate = _transactionToEdit.Date;
                cmbType.SelectedIndex = _transactionToEdit.Type == "Income" ? 1 : 0;
                txtAmount.Text = _transactionToEdit.Amount.ToString();
                txtDescription.Text = _transactionToEdit.Description;
            }
        }

        private void LoadCategories(string? typeFilter = null)
        {
            var allCategories = _categoryService.GetAll();

            if (typeFilter == "Income")
                cmbCategory.ItemsSource = allCategories.Where(c => c.Type == "Income").ToList();
            else if (typeFilter == "Expense")
                cmbCategory.ItemsSource = allCategories.Where(c => c.Type == "Expense").ToList();
            else
                cmbCategory.ItemsSource = allCategories;

            cmbCategory.DisplayMemberPath = "Name";
        }

        private void CmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbType.SelectedItem is ComboBoxItem item)
            {
                string type = item.Tag?.ToString() ?? "Expense";
                LoadCategories(type);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null) return;

            if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму!", "Ошибка");
                return;
            }

            if (cmbCategory.SelectedItem is not Category selectedCategory)
            {
                MessageBox.Show("Выберите категорию!", "Ошибка");
                return;
            }

            var transaction = _transactionToEdit ?? new FinancialTransaction();

            transaction.Date = dpDate.SelectedDate ?? DateTime.Now;
            transaction.Type = ((ComboBoxItem)cmbType.SelectedItem).Tag?.ToString() ?? "Expense";
            transaction.Amount = amount;
            transaction.Description = txtDescription.Text;
            transaction.CategoryId = selectedCategory.Id;
            transaction.UserId = App.CurrentUser.Id;

            if (_transactionToEdit == null)
                _transactionService.Add(transaction);
            else
                _transactionService.Update(transaction);

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}