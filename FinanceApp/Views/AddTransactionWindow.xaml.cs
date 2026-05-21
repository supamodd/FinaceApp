using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            _categoryService.EnsureDefaultCategoriesExist();
            LoadCategories();

            _transactionToEdit = transaction;

            if (_transactionToEdit != null)
            {
                Title = "Редактирование операции";
                dpDate.SelectedDate = _transactionToEdit.Date;
                cmbType.SelectedIndex = _transactionToEdit.Type == "Income" ? 1 : 0;
                txtAmount.Text = _transactionToEdit.Amount.ToString();
                txtDescription.Text = _transactionToEdit.Description;
            }

            cmbDayOfMonth.ItemsSource = Enumerable.Range(1, 31);
            cmbDayOfMonth.SelectedIndex = DateTime.Now.Day - 1;

            dpStartDate.SelectedDate = DateTime.Now;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
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

        private void ChkRecurring_Changed(object sender, RoutedEventArgs e)
        {
            RecurringPanel.Visibility = chkRecurring.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;
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

            if (chkRecurring.IsChecked == true)
            {
                var recurring = new RecurringTransaction
                {
                    UserId = App.CurrentUser.Id,
                    CategoryId = selectedCategory.Id,
                    Amount = amount,
                    Type = transaction.Type,
                    Description = txtDescription.Text,
                    Frequency = ((ComboBoxItem)cmbFrequency.SelectedItem).Tag?.ToString() ?? "Monthly",
                    StartDate = dpStartDate.SelectedDate ?? DateTime.Now,
                    NextOccurrence = dpStartDate.SelectedDate ?? DateTime.Now,
                    DayOfMonth = cmbDayOfMonth.SelectedIndex + 1,
                    IsActive = true
                };

                MessageBox.Show("Повторяющаяся транзакция сохранена в базу!",
                              "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
            }

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
