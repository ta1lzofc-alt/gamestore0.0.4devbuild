using System.Windows;
using System.Windows.Media;

namespace GameStore.Desktop;

public enum AppTheme
{
    Dark,
    Light
}

public static class ThemeManager
{
    public static AppTheme CurrentTheme { get; private set; }
        = AppTheme.Dark;

    public static void ApplyTheme(AppTheme theme)
    {
        CurrentTheme = theme;

        bool dark = theme == AppTheme.Dark;

        var resources = Application.Current.Resources;

        resources["AppBackground"] = Brush(
            dark ? "#171A21" : "#F3F3F3");

        resources["AppPanel"] = Brush(
            dark ? "#1B2838" : "#FFFFFF");

        resources["AppCard"] = Brush(
            dark ? "#243447" : "#E5E5E5");

        resources["AppForeground"] = Brush(
            dark ? "#FFFFFF" : "#171717");

        resources["AppSecondary"] = Brush(
            dark ? "#B8C7D9" : "#666666");

        resources["AppBorder"] = Brush(
            dark ? "#3B526B" : "#D0D0D0");

        resources["AppInput"] = Brush(
            dark ? "#101820" : "#FFFFFF");

        resources["AppAccent"] = Brush("#66C0F4");
    }

    private static SolidColorBrush Brush(string hex)
    {
        return new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(hex));
    }
}