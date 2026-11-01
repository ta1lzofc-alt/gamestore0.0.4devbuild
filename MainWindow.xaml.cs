using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GameStore.Desktop.Data;
using GameStore.Desktop.Models;

namespace GameStore.Desktop;

public partial class MainWindow : Window
{
    private readonly GameRepository _repository = new();
    private readonly LibraryRepository _libraryRepository = new();
    private readonly OrderRepository _orderRepository = new();
    private readonly UserRepository _userRepository = new();

    private readonly long _currentUserId;

    public bool IsLogoutRequested { get; private set; }

    public ObservableCollection<Game> Games { get; } = new();
    public ObservableCollection<Game> CartGames { get; } = new();
    public ObservableCollection<Game> LibraryGames { get; } = new();

    public MainWindow(long userId)
    {
        if (userId <= 0)
            throw new ArgumentException(
                "Некорректный ID пользователя.",
                nameof(userId));

        _currentUserId = userId;

        InitializeComponent();
        DataContext = this;

        Loaded += async (_, _) =>
        {
            await LoadGamesAsync();
            await LoadLibraryAsync();
            UpdateCart();

            ThemeSelector.SelectedIndex =
                ThemeManager.CurrentTheme == AppTheme.Dark ? 0 : 1;
        };
    }

    private async Task LoadGamesAsync()
    {
        try
        {
            var games = await _repository.GetGamesAsync(SearchBox.Text);

            Games.Clear();

            foreach (var game in games)
                Games.Add(game);

            StatusText.Text = $"Найдено игр: {Games.Count}";
        }
        catch (Exception ex)
        {
            StatusText.Text = "Ошибка загрузки каталога.";

            MessageBox.Show(
                ex.Message,
                "GameStore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task LoadLibraryAsync()
    {
        try
        {
            var games =
                await _libraryRepository.GetUserGamesAsync(_currentUserId);

            LibraryGames.Clear();

            foreach (var game in games)
                LibraryGames.Add(game);

            LibraryCountText.Text =
                $"Игр в библиотеке: {LibraryGames.Count}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Ошибка загрузки библиотеки",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            var profile =
                await _userRepository.GetUserAsync(_currentUserId);

            if (profile == null)
            {
                MessageBox.Show("Пользователь не найден.");
                return;
            }

            ProfileUsername.Text = profile.Username;
            ProfileEmail.Text = profile.Email;
            ProfileCreatedAt.Text =
                profile.CreatedAt.ToString("dd.MM.yyyy");
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Ошибка загрузки профиля",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ShowPage(string page)
    {
        StorePage.Visibility = Visibility.Collapsed;
        LibraryPage.Visibility = Visibility.Collapsed;
        CartPage.Visibility = Visibility.Collapsed;
        ProfilePage.Visibility = Visibility.Collapsed;
        SettingsPage.Visibility = Visibility.Collapsed;

        switch (page)
        {
            case "Home":
            case "Store":
                StorePage.Visibility = Visibility.Visible;
                PageTitle.Text =
                    page == "Home" ? "Главная" : "Каталог игр";
                break;

            case "Library":
                LibraryPage.Visibility = Visibility.Visible;
                PageTitle.Text = "Моя библиотека";
                break;

            case "Cart":
                CartPage.Visibility = Visibility.Visible;
                PageTitle.Text = "Корзина";
                break;

            case "Profile":
                ProfilePage.Visibility = Visibility.Visible;
                PageTitle.Text = "Профиль";
                break;

            case "Settings":
                SettingsPage.Visibility = Visibility.Visible;
                PageTitle.Text = "Настройки";
                break;
        }
    }

    private void Home_Click(object sender, RoutedEventArgs e)
    {
        ShowPage("Home");
    }

    private void Store_Click(object sender, RoutedEventArgs e)
    {
        ShowPage("Store");
    }

    private async void Library_Click(object sender, RoutedEventArgs e)
    {
        ShowPage("Library");
        await LoadLibraryAsync();
    }

    private void Cart_Click(object sender, RoutedEventArgs e)
    {
        ShowPage("Cart");
    }

    private async void Profile_Click(object sender, RoutedEventArgs e)
    {
        ShowPage("Profile");
        await LoadProfileAsync();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        ShowPage("Settings");
    }

    private void ThemeSelector_SelectionChanged(
        object sender,
        System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ThemeSelector.SelectedIndex == 0)
        {
            ThemeManager.ApplyTheme(AppTheme.Dark);
        }
        else if (ThemeSelector.SelectedIndex == 1)
        {
            ThemeManager.ApplyTheme(AppTheme.Light);
        }
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadGamesAsync();
    }

    private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await LoadGamesAsync();
    }

    private void AddToCart_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Game game)
        {
            if (LibraryGames.Any(g => g.GameId == game.GameId))
            {
                MessageBox.Show(
                    "Эта игра уже есть в вашей библиотеке.");
                return;
            }

            if (CartGames.Any(g => g.GameId == game.GameId))
            {
                MessageBox.Show(
                    "Эта игра уже находится в корзине.");
                return;
            }

            CartGames.Add(game);
            UpdateCart();
        }
    }

    private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Game game)
        {
            CartGames.Remove(game);
            UpdateCart();
        }
    }

    private void UpdateCart()
    {
        CartButton.Content = $"🛒   Корзина ({CartGames.Count})";

        decimal total = CartGames.Sum(g => g.Price);
        CartTotalText.Text = $"Итого: {total:N2} ₽";
    }

    private async void Checkout_Click(object sender, RoutedEventArgs e)
    {
        if (CartGames.Count == 0)
        {
            MessageBox.Show("Корзина пуста.");
            return;
        }

        var alreadyOwned = CartGames
            .Where(cartGame => LibraryGames.Any(
                libraryGame => libraryGame.GameId == cartGame.GameId))
            .ToList();

        if (alreadyOwned.Count > 0)
        {
            string titles = string.Join(
                ", ",
                alreadyOwned.Select(game => game.Title));

            MessageBox.Show(
                $"Эти игры уже есть в вашей библиотеке: {titles}",
                "GameStore",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        try
        {
            long orderId = await _orderRepository.CreateOrderAsync(
                _currentUserId,
                CartGames.ToList());

            CartGames.Clear();
            UpdateCart();

            await LoadLibraryAsync();

            MessageBox.Show(
                $"Заказ №{orderId} успешно оформлен!\nИгры добавлены в библиотеку.",
                "GameStore",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось оформить заказ:\n{ex.Message}",
                "Ошибка оформления заказа",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Вы действительно хотите выйти из аккаунта?",
            "GameStore",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        IsLogoutRequested = true;
        Close();
    }
}
