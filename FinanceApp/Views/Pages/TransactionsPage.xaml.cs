using System.Windows;
using System.Windows.Controls;
using FinanceApp.Data.Models;
using FinanceApp.Services.Transactions;

namespace FinanceApp.Views.Pages
{
    public partial class TransactionsPage : UserControl
    {
        private readonly TransactionService _service = new TransactionService();

        public TransactionsPage()
        {
            InitializeComponent();
            LoadTransactions();
        }

        private void LoadTransactions()
        {
            if (App.CurrentUser == null) return;
            dgTransactions.ItemsSource = _service.GetAll(App.CurrentUser.Id);
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
                MessageBox.Show("Выберите операцию для редактирования", "Внимание");
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
                MessageBox.Show("Выберите операцию для удаления", "Внимание");
            }
        }

        // ← Вот этот метод был пропущен
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTransactions();
        }
    }
}