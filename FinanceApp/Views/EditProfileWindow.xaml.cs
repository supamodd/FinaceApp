using System;
using System.Windows;
using System.Windows.Input;
using FinanceApp.Data.Models;
using FinanceApp.Services;
using Microsoft.Win32;

namespace FinanceApp.Views
{
    public partial class EditProfileWindow : Window
    {
        private readonly User _currentUser;
        private string? _newAvatarPath;

        public EditProfileWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;

            txtFullName.Text = user.FullName;
            txtEmail.Text = user.Email;

            if (!string.IsNullOrEmpty(user.AvatarPath) && System.IO.File.Exists(user.AvatarPath))
                imgAvatar.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(user.AvatarPath));
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void BtnChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Изображения (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (dialog.ShowDialog() == true)
            {
                _newAvatarPath = dialog.FileName;
                imgAvatar.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(_newAvatarPath));
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _currentUser.FullName = txtFullName.Text.Trim();
            _currentUser.Email = txtEmail.Text.Trim();

            if (!string.IsNullOrEmpty(_newAvatarPath))
                _currentUser.AvatarPath = _newAvatarPath;

            if (!string.IsNullOrEmpty(txtNewPassword.Password))
            {
                _currentUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(txtNewPassword.Password);
            }

            using var context = new FinanceApp.Data.AppDbContext();
            context.Users.Update(_currentUser);
            context.SaveChanges();

            MessageBox.Show("Профиль успешно обновлён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
