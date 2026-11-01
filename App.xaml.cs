
using System.Windows;
using GameStore.Desktop.Views;

namespace GameStore.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        while (true)
        {
            var loginWindow = new LoginWindow();
            bool? loginResult = loginWindow.ShowDialog();

            if (loginResult != true)
            {
                Shutdown();
                return;
            }

            var mainWindow = new MainWindow(
                loginWindow.AuthenticatedUserId);

            MainWindow = mainWindow;
            mainWindow.ShowDialog();

            if (!mainWindow.IsLogoutRequested)
            {
                Shutdown();
                return;
            }
        }
    }
}
