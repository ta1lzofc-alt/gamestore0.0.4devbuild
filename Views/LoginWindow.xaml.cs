
using System.Windows;
using GameStore.Desktop.Data;

namespace GameStore.Desktop.Views;

public partial class LoginWindow : Window
{
    private readonly AuthRepository _authRepository = new();

    public long AuthenticatedUserId { get; private set; }

    public LoginWindow()
    {
        InitializeComponent();
        LoginInput.Focus();
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = "";

        string login = LoginInput.Text.Trim();
        string password = PasswordInput.Password;

        if (string.IsNullOrWhiteSpace(login) ||
            string.IsNullOrWhiteSpace(password))
        {
            ErrorText.Text = "Заполните все поля.";
            return;
        }

        try
        {
            AuthenticatedUserId =
                await _authRepository.LoginAsync(login, password)
                ?? 0;

            if (AuthenticatedUserId == 0)
            {
                ErrorText.Text = "Неверный логин или пароль.";
                return;
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Ошибка входа",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void Register_Click(object sender, RoutedEventArgs e)
    {
        var registerWindow = new RegisterWindow
        {
            Owner = this
        };

        registerWindow.ShowDialog();
    }
}
