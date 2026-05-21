using System;
using System.Windows;
using System.Windows.Input;
using FinanceApp.Services;
using FinanceApp.Views;

namespace FinanceApp.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService = new AuthService();

        public LoginWindow()
        {
            InitializeComponent();
            // Allow dragging the borderless window
            this.MouseLeftButtonDown += (s, e) => { if (e.ChangedButton == MouseButton.Left) DragMove(); };
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = txtLogin.Text.Trim();
                string password = txtPassword.Password;

                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    txtError.Text = "Введите логин и пароль!";
                    return;
                }

                var user = _authService.Login(login, password);
                if (user != null)
                {
                    App.CurrentUser = user;

                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    txtError.Text = "Неверный логин или пароль!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе:\n\n{ex.Message}\n\n{ex.StackTrace}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = txtLogin.Text.Trim();
                string password = txtPassword.Password;

                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    txtError.Text = "Введите логин и пароль!";
                    return;
                }

                var user = _authService.Register(login, password, login);
                if (user != null)
                {
                    txtError.Foreground = System.Windows.Media.Brushes.Green;
                    txtError.Text = "✅ Регистрация успешна! Теперь нажмите «Войти»";
                }
                else
                {
                    txtError.Text = "Пользователь с таким логином уже существует!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации:\n\n{ex.Message}\n\n{ex.StackTrace}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtLogin_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
