using System.Windows.Input;
using FinanceApp.Data;
using FinanceApp.Data.Models;
using FinanceApp.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FinanceApp.Views
{
    public partial class FamilyPage : Page
    {
        private readonly AppDbContext _context = new AppDbContext();
        private readonly FamilyService _familyService;
        private int _currentUserId;
        private FamilyMember _selectedMember;

        public FamilyPage()
        {
            InitializeComponent();
            _familyService = new FamilyService(_context);

            // Берём ID текущего пользователя
            _currentUserId = App.CurrentUser?.Id ?? 0;

            LoadFamilyInfo();
        }

        private async void LoadFamilyInfo()
        {
            try
            {
                var families = await _familyService.GetUserFamiliesAsync(_currentUserId);

                if (families.Any())
                {
                    NotInFamilyPanel.Visibility = Visibility.Collapsed;
                    InFamilyPanel.Visibility = Visibility.Visible;

                    var family = families.First();
                    FamilyNameText.Text = family.Name;

                    var members = await _familyService.GetFamilyMembersAsync(family.Id);
                    MembersGrid.ItemsSource = members;

                    var myRole = members.FirstOrDefault(m => m.UserId == _currentUserId)?.Role;
                    RoleText.Text = $"Ваша роль: {myRole}";
                }
                else
                {
                    NotInFamilyPanel.Visibility = Visibility.Visible;
                    InFamilyPanel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
            }
        }

        private async void CreateFamily_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FamilyNameBox.Text))
                {
                    MessageBox.Show("Введите название семьи");
                    return;
                }

                await _familyService.CreateFamilyAsync(_currentUserId, FamilyNameBox.Text);
                MessageBox.Show("Семья успешно создана!");
                LoadFamilyInfo();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                if (ex.InnerException != null)
                    error += "\n\nInner: " + ex.InnerException.Message;

                MessageBox.Show(error, "Ошибка создания семьи");
            }
        }

        private async void JoinByCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(InviteCodeBox.Text)) return;

                bool success = await _familyService.JoinByInvitationCodeAsync(_currentUserId, InviteCodeBox.Text);
                MessageBox.Show(success ? "Вы присоединились к семье!" : "Неверный код");
                LoadFamilyInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        private async void Invite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var families = await _familyService.GetUserFamiliesAsync(_currentUserId);
                if (!families.Any()) return;

                var code = await _familyService.GenerateInvitationCodeAsync(families.First().Id, _currentUserId);

                // Копируем в буфер обмена
                Clipboard.SetText(code);

                MessageBox.Show($"Код приглашения скопирован в буфер обмена!\n\n{code}\n\nДействителен 7 дней",
                                "Приглашение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }

        // Покинуть семью
        private async void LeaveFamily_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите покинуть семью?", "Подтверждение",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                await _familyService.LeaveFamilyAsync(_currentUserId, _currentUserId); // упрощённо
                MessageBox.Show("Вы покинули семью");
                LoadFamilyInfo();
            }
        }
        private void MembersGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _selectedMember = MembersGrid.SelectedItem as FamilyMember;
        }
        // Сменить роль
        private async void ChangeRole_Click(object sender, RoutedEventArgs e)
        {
            var selected = MembersGrid.SelectedItem as FamilyMember;
            if (selected == null) return;

            // Создаём окно выбора роли
            var roleWindow = new Window
            {
                Title = "Выберите роль",
                Width = 300,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var comboBox = new ComboBox
            {
                ItemsSource = Enum.GetValues(typeof(FamilyRole)),
                SelectedItem = selected.Role,
                Margin = new Thickness(20),
                Height = 35
            };

            var button = new Button
            {
                Content = "Сохранить",
                Width = 120,
                Height = 35,
                Margin = new Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            button.Click += async (s, args) =>
            {
                var newRole = (FamilyRole)comboBox.SelectedItem;

                bool success = await _familyService.ChangeRoleAsync(
                    (await _familyService.GetUserFamiliesAsync(_currentUserId)).First().Id,
                    selected.UserId, newRole, _currentUserId);

                if (success)
                {
                    roleWindow.Close();
                    LoadFamilyInfo();
                }
                else
                {
                    MessageBox.Show("Нет прав на изменение роли");
                }
            };

            var panel = new StackPanel();
            panel.Children.Add(comboBox);
            panel.Children.Add(button);
            roleWindow.Content = panel;
            roleWindow.ShowDialog();
        }

        // Исключить участника
        private async void RemoveMember_Click(object sender, RoutedEventArgs e)
        {
            var selected = MembersGrid.SelectedItem as FamilyMember;
            if (selected == null) return;

            if (MessageBox.Show($"Исключить {selected.User?.FullName ?? selected.User?.Login}?",
                "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            bool success = await _familyService.RemoveMemberAsync(
                (await _familyService.GetUserFamiliesAsync(_currentUserId)).First().Id,
                selected.UserId, _currentUserId);

            if (success)
                LoadFamilyInfo();
            else
                MessageBox.Show("Нет прав или нельзя исключить владельца");
        }
    }
}