using System;
using System.Linq;
using System.Runtime.InteropServices;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Launcher.Views;

public partial class Main : Window
{
    public readonly ViewModels.Main ViewModel = new();

    public Main()
    {
        DataContext = ViewModel;

        InitializeComponent();

        PlayButtonHost.PointerEntered += (_, _) => UpdatePlayButtonImage(isHovered: true);
        PlayButtonHost.PointerExited += (_, _) => UpdatePlayButtonImage(isHovered: false);

        SetupSidebarIcon(NewServerButton, NewServerIcon, "New");
        SetupSidebarIcon(SettingsButton, SettingsIcon, "Settings");
        SetupSidebarIcon(DiscordButton, DiscordIcon, "Discord");
        SetupSidebarIcon(RedditButton, RedditIcon, "Reddit");

        ViewModel.PropertyChanged += (_, e) =>
        {
            UpdatePlayButtonImage(isHovered: false);
        };
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        ViewModel.OnLoad();
        SetRandomBackground();
        UpdatePlayButtonImage(isHovered: false);
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        DisableMaximizeButton();
    }

    private void DisableMaximizeButton()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;

        if (TryGetPlatformHandle()?.Handle is not { } hwnd || hwnd == IntPtr.Zero)
            return;

        const int GWL_STYLE = -16;
        const int WS_MAXIMIZEBOX = 0x00010000;

        var style = GetWindowLong(hwnd, GWL_STYLE);
        SetWindowLong(hwnd, GWL_STYLE, style & ~WS_MAXIMIZEBOX);

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_FRAMECHANGED = 0x0020;

        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
            SWP_NOSIZE | SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int x, int y, int cx, int cy, uint uFlags);

    private void SetupSidebarIcon(Button button, Image icon, string name)
    {
        icon.Source = LoadAltButton(name, hovered: false);
        button.PointerEntered += (_, _) => icon.Source = LoadAltButton(name, hovered: true);
        button.PointerExited += (_, _) => icon.Source = LoadAltButton(name, hovered: false);
    }

    private static Bitmap LoadAltButton(string name, bool hovered)
    {
        var file = hovered ? $"{name}_Hover.png" : $"{name}.png";

        return new Bitmap(AssetLoader.Open(
            new Uri($"avares://Launcher/Assets/AltButtons/{file}")));
    }

    private void UpdatePlayButtonImage(bool isHovered)
    {
        var name = isHovered ? "Play_Button_Hover.png" : "Play_Button.png";

        PlayButtonImage.Source = new Bitmap(AssetLoader.Open(
            new Uri($"avares://Launcher/Assets/Frame/{name}")));
    }

    private void SetRandomBackground()
    {
        var index = Random.Shared.Next(1, 21);
        var uri = new Uri($"avares://Launcher/Assets/Backgrounds/Back{index}.png");
        using var stream = AssetLoader.Open(uri);
        HeroGrid.Background = new ImageBrush(new Bitmap(stream)) { Stretch = Stretch.UniformToFill };
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (ViewModel.Servers.Any(s => s.IsDownloading))
        {
            e.Cancel = true;
            App.AddNotification(App.GetText("Text.Downloading.OnClose"), true);
        }
        else
        {
            base.OnClosing(e);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            App.CancelPopup();
            e.Handled = true;

            return;
        }

        base.OnKeyDown(e);
    }
}
