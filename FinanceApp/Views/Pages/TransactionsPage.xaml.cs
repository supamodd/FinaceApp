using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FinanceApp.Data.Models;
using FinanceApp.Services.Categories;
using FinanceApp.Services.Transactions;

namespace FinanceApp.Views.Pages
{
    public partial class TransactionsPage : UserControl
    {
        private readonly TransactionService _service = new TransactionService();
        private readonly CategoryService _categoryService = new CategoryService();
        private List<FinancialTransaction> _allTransactions = new();

        public TransactionsPage()
        {
            InitializeComponent();
            this.Loaded += TransactionsPage_Loaded;
        }

        private void TransactionsPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories();
            LoadTransactions();
        }

        private void LoadCategories()
        {
            var categories = _categoryService.GetAll();
            var allItem = new Category { Id = 0, Name = "Все категории" };
            var list = new List<Category> { allItem };
            list.AddRange(categories);
            cmbCategoryFilter.ItemsSource = list;
            cmbCategoryFilter.SelectedIndex = 0;
        }

        private void LoadTransactions()
        {
            if (App.CurrentUser == null) return;
            _allTransactions = _service.GetAll(App.CurrentUser.Id)
                .OrderByDescending(t => t.Date)
                .ToList();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allTransactions.AsEnumerable();

            // Фильтр по типу
            if (cmbTypeFilter.SelectedIndex == 1) // Доход
                filtered = filtered.Where(t => t.Type == "Income");
            else if (cmbTypeFilter.SelectedIndex == 2) // Расход
                filtered = filtered.Where(t => t.Type == "Expense");

            // Фильтр по категории
            if (cmbCategoryFilter.SelectedItem is Category cat && cat.Id != 0)
                filtered = filtered.Where(t => t.CategoryId == cat.Id);

            // Фильтр по дате "от"
            if (dpFilterFrom.SelectedDate is DateTime fromDate)
                filtered = filtered.Where(t => t.Date >= fromDate);

            // Фильтр по дате "до"
            if (dpFilterTo.SelectedDate is DateTime toDate)
                filtered = filtered.Where(t => t.Date <= toDate);

            // Поиск по описанию
            string search = txtSearch.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(t =>
                    (t.Description != null && t.Description.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (t.Category?.Name != null && t.Category.Name.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            var result = filtered.ToList();
            dgTransactions.ItemsSource = result;

            // Счётчик
            txtCount.Text = $"Найдено: {result.Count} из {_allTransactions.Count}";
        }

        // ── Обработчики фильтров ──
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void Filter_Changed(object sender, object e)
        {
            if (_allTransactions == null || _allTransactions.Count == 0) return;
            ApplyFilters();
        }

        private void BtnClearDates_Click(object sender, RoutedEventArgs e)
        {
            dpFilterFrom.SelectedDate = null;
            dpFilterTo.SelectedDate = null;
            ApplyFilters();
        }

        // ── Действия ──
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddTransactionWindow();
            if (window.ShowDialog() == true)
                LoadTransactions();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgTransactions.SelectedItem is FinancialTransaction selected)
            {
                var window = new AddTransactionWindow(selected);
                if (window.ShowDialog() == true)
                    LoadTransactions();
            }
            else
            {
                ToastNotification.Show("Выберите операцию для редактирования", "Внимание");
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgTransactions.SelectedItem is FinancialTransaction selected)
            {
                if (MessageBox.Show("Удалить эту операцию?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _service.Delete(selected.Id);
                    LoadTransactions();
                }
            }
            else
            {
                ToastNotification.Show("Выберите операцию для удаления", "Внимание");
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            // Сброс всех фильтров
            txtSearch.Text = "";
            cmbTypeFilter.SelectedIndex = 0;
            cmbCategoryFilter.SelectedIndex = 0;
            dpFilterFrom.SelectedDate = null;
            dpFilterTo.SelectedDate = null;
            LoadTransactions();
        }
    }
}