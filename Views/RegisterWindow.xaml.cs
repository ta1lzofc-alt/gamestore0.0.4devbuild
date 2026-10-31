
using System.Text.RegularExpressions;
using System.Windows;
using GameStore.Desktop.Data;

namespace GameStore.Desktop.Views;

public partial class RegisterWindow : Window
{
    private readonly AuthRepository _authRepository = new();

    public RegisterWindow()
    {
        InitializeComponent();
        UsernameInput.Focus();
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorBorder.Visibility = Visibility.Visible;
    }

    private void ClearError()
    {
        ErrorText.Text = "";
        ErrorBorder.Visibility = Visibility.Collapsed;
    }

    private async void Register_Click(object sender, RoutedEventArgs e)
    {
        ClearError();

        string username = UsernameInput.Text.Trim();
        string email = EmailInput.Text.Trim();
        string password = PasswordInput.Password;
        string confirmPassword = ConfirmPasswordInput.Password;

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            ShowError("Заполните все поля.");
            return;
        }

        if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]{3,24}$"))
        {
            ShowError(
                "Логин: 3–24 символа, латинские буквы, цифры и _.");
            return;
        }

        if (!Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            ShowError("Введите корректный email.");
            return;
        }

        if (password.Length < 8)
        {
            ShowError(
                "Пароль должен содержать минимум 8 символов.");
            return;
        }

        if (password != confirmPassword)
        {
            ShowError("Пароли не совпадают.");
            return;
        }

        RegisterButton.IsEnabled = false;
        RegisterButton.Content = "СОЗДАНИЕ АККАУНТА...";

        try
        {
            await _authRepository.RegisterAsync(
                username,
                email,
                password);

            MessageBox.Show(
                "Аккаунт успешно создан! Теперь можно войти.",
                "GameStore",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Close();
        }
        catch (InvalidOperationException ex)
        {
            ShowError(ex.Message);
        }
        catch (Exception)
        {
            ShowError(
                "Не удалось создать аккаунт. Проверьте подключение к базе данных.");
        }
        finally
        {
            RegisterButton.IsEnabled = true;
            RegisterButton.Content = "ЗАРЕГИСТРИРОВАТЬСЯ";
        }
    }
}
