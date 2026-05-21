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
                    RoleText.Text = $"Ваша роль: {GetRoleDisplayName(myRole ?? FamilyRole.Member)}";
                }
                else
                {
                    NotInFamilyPanel.Visibility = Visibility.Visible;
                    InFamilyPanel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                ToastNotification.Show("Ошибка", "Ошибка загрузки: " + ex.Message, ToastType.Error);
            }
        }

        private string GetRoleDisplayName(FamilyRole role)
        {
            var field = role.GetType().GetField(role.ToString());
            if (field != null)
            {
                var attribute = field.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false)
                    .FirstOrDefault() as System.ComponentModel.DataAnnotations.DisplayAttribute;
                return attribute?.Name ?? role.ToString();
            }
            return role.ToString();
        }

        private async void CreateFamily_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FamilyNameBox.Text))
                {
                    ToastNotification.Show("Внимание", "Введите название семьи", ToastType.Warning);
                    return;
                }

                await _familyService.CreateFamilyAsync(_currentUserId, FamilyNameBox.Text);
                ToastNotification.Show("Успех", "Семья успешно создана!", ToastType.Success);
                LoadFamilyInfo();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                if (ex.InnerException != null)
                    error += "\n" + ex.InnerException.Message;
                ToastNotification.Show("Ошибка", error, ToastType.Error);
            }
        }

        private async void JoinByCode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(InviteCodeBox.Text)) return;

                bool success = await _familyService.JoinByInvitationCodeAsync(_currentUserId, InviteCodeBox.Text);
                if (success)
                    ToastNotification.Show("Успех", "Вы присоединились к семье!", ToastType.Success);
                else
                    ToastNotification.Show("Ошибка", "Неверный код приглашения", ToastType.Error);
                LoadFamilyInfo();
            }
            catch (Exception ex)
            {
                ToastNotification.Show("Ошибка", ex.Message, ToastType.Error);
            }
        }

        private async void Invite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var families = await _familyService.GetUserFamiliesAsync(_currentUserId);
                if (!families.Any()) return;

                var code = await _familyService.GenerateInvitationCodeAsync(families.First().Id, _currentUserId);
                Clipboard.SetText(code);

                ToastNotification.Show("Код скопирован!", $"{code}\nДействителен 7 дней", ToastType.Info);
            }
            catch (Exception ex)
            {
                ToastNotification.Show("Ошибка", ex.Message, ToastType.Error);
            }
        }

        private async void LeaveFamily_Click(object sender, RoutedEventArgs e)
        {
            ToastNotification.ShowConfirm(
                "Покинуть семью?",
                "Вы уверены, что хотите покинуть семью?",
                async () =>
                {
                    await _familyService.LeaveFamilyAsync(_currentUserId, _currentUserId);
                    ToastNotification.Show("Готово", "Вы покинули семью", ToastType.Info);
                    LoadFamilyInfo();
                },
                "Да, покинуть"
            );
        }

        private void MembersGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _selectedMember = MembersGrid.SelectedItem as FamilyMember;
        }

        private async void ChangeRole_Click(object sender, RoutedEventArgs e)
        {
            var selected = MembersGrid.SelectedItem as FamilyMember;
            if (selected == null) return;

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
                    ToastNotification.Show("Успех", "Роль изменена", ToastType.Success);
                    LoadFamilyInfo();
                }
                else
                {
                    ToastNotification.Show("Ошибка", "Нет прав на изменение роли", ToastType.Error);
                }
            };

            var panel = new StackPanel();
            panel.Children.Add(comboBox);
            panel.Children.Add(button);
            roleWindow.Content = panel;
            roleWindow.ShowDialog();
        }

        private async void RemoveMember_Click(object sender, RoutedEventArgs e)
        {
            var selected = MembersGrid.SelectedItem as FamilyMember;
            if (selected == null) return;

            string memberName = selected.User?.FullName ?? selected.User?.Login ?? "участника";

            ToastNotification.ShowConfirm(
                "Исключить участника?",
                $"Исключить {memberName} из семьи?",
                async () =>
                {
                    bool success = await _familyService.RemoveMemberAsync(
                        (await _familyService.GetUserFamiliesAsync(_currentUserId)).First().Id,
                        selected.UserId, _currentUserId);

                    if (success)
                    {
                        ToastNotification.Show("Готово", $"{memberName} исключён из семьи", ToastType.Success);
                        LoadFamilyInfo();
                    }
                    else
                    {
                        ToastNotification.Show("Ошибка", "Нет прав или нельзя исключить владельца", ToastType.Error);
                    }
                },
                "Да, исключить"
            );
        }
    }
}