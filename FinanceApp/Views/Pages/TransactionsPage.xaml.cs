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
                .OrderByDescending(t => t.Date).ToList();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var filtered = _allTransactions.AsEnumerable();

            if (cmbTypeFilter.SelectedIndex == 1)
                filtered = filtered.Where(t => t.Type == "Income");
            else if (cmbTypeFilter.SelectedIndex == 2)
                filtered = filtered.Where(t => t.Type == "Expense");

            if (cmbCategoryFilter.SelectedItem is Category cat && cat.Id != 0)
                filtered = filtered.Where(t => t.CategoryId == cat.Id);

            if (dpFilterFrom.SelectedDate is DateTime fromDate)
                filtered = filtered.Where(t => t.Date >= fromDate);

            if (dpFilterTo.SelectedDate is DateTime toDate)
                filtered = filtered.Where(t => t.Date <= toDate);

            string search = txtSearch.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(t =>
                    (t.Description != null && t.Description.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (t.Category?.Name != null && t.Category.Name.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            var result = filtered.ToList();
            dgTransactions.ItemsSource = result;
            txtCount.Text = $"Найдено: {result.Count} из {_allTransactions.Count}";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();

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
                ToastNotification.Show("Внимание", "Выберите операцию для редактирования", ToastType.Warning);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgTransactions.SelectedItem is FinancialTransaction selected)
            {
                ToastNotification.ShowConfirm(
                    "Удалить операцию?",
                    $"{selected.Amount:N0} ₽ — {selected.Description ?? "без описания"}",
                    () =>
                    {
                        _service.Delete(selected.Id);
                        LoadTransactions();
                        ToastNotification.Show("Удалено", "Операция удалена", ToastType.Success);
                    },
                    "Да, удалить"
                );
            }
            else
            {
                ToastNotification.Show("Внимание", "Выберите операцию для удаления", ToastType.Warning);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            cmbTypeFilter.SelectedIndex = 0;
            cmbCategoryFilter.SelectedIndex = 0;
            dpFilterFrom.SelectedDate = null;
            dpFilterTo.SelectedDate = null;
            LoadTransactions();
        }
    }
}