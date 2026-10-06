// Added 2026-10-05. Normal focusable settings window; gameplay overlays do not activate. GPL-3.0.
using System.Windows;
using System.Windows.Controls;
using UMADOverlay.Models;
using UMADOverlay.Services;

namespace UMADOverlay.Views;

public sealed class SettingsWindow : Window
{
    public SettingsWindow(OverlayController controller)
    {
        Title = "Kefka Split · Settings / 設定";
        Width = 420; SizeToContent = SizeToContent.Height;
        ResizeMode = ResizeMode.NoResize; WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Topmost = true; Background = OverlayWindow.Brush("#1C2730");
        Foreground = OverlayWindow.Brush("#F0EEE8");
        var panel = new StackPanel { Margin = new Thickness(22) };
        Content = panel;
        void Label(string text) => panel.Children.Add(new TextBlock {
            Text = text, Margin = new Thickness(0,0,0,8), TextWrapping = TextWrapping.Wrap });
        Label("Language / 語言 / 言語");
        var languages = new ComboBox { ItemsSource = new[] { "繁體中文", "日本語", "English" },
            SelectedIndex = (int)controller.Settings.Language, Margin = new Thickness(0,0,0,18) };
        languages.SelectionChanged += (_, _) => controller.SetLanguage((Lang)languages.SelectedIndex);
        panel.Children.Add(languages);
        Label("Background opacity / 背景不透明度");
        var opacity = new Slider { Minimum = 20, Maximum = 100, TickFrequency = 5, IsSnapToTickEnabled = true,
            Value = controller.Settings.BackgroundOpacity * 100, Margin = new Thickness(0,0,0,18) };
        opacity.ValueChanged += (_, _) => { controller.Settings.BackgroundOpacity = opacity.Value / 100; controller.ApplyPreferences(); };
        panel.Children.Add(opacity);
        var locked = new CheckBox { Content = "Lock window positions and sizes / 鎖定位置及大小",
            IsChecked = controller.Settings.LayoutLocked, Foreground = Foreground, Margin = new Thickness(0,0,0,18) };
        locked.Click += (_, _) => { controller.Settings.LayoutLocked = locked.IsChecked == true; controller.ApplyPreferences(); };
        panel.Children.Add(locked);
        void Action(string caption, Action action)
        {
            var button = new Button { Content = caption, Height = 32, Margin = new Thickness(0,0,0,8) };
            button.Click += (_, _) => action(); panel.Children.Add(button);
        }
        Action("Recover positions / 重置視窗位置及大小", controller.RecoverPositions);
        Action("Collapse to icon / 縮成圖示", controller.Collapse);
        Label("Drag either title bar to move it. Drag its edges to resize.\n拖動標題列移動；拖動邊緣調整大小。\n\nClick the question-mark icon to restore both windows. Drag the icon to move it.\n點擊紅色問號圖示同時展開兩個視窗；拖動圖示可移動。\n\nBlue circle = true · Red ? = false. Hover a tile for its mechanic.\n藍色圓形 = 真 · 紅色 ? = 假；滑鼠停留可查看按鈕說明。\n\nAdapted from AweiYourdog/FF14-Kefka-P4-for-PC · GPL-3.0.\nNo warranty. See LICENSE and NOTICE in the source package.");
        if (controller.SettingsError != null) Label(controller.SettingsError);
        Action("Close settings / 關閉設定", Close);
        Action("Exit application / 結束程式", controller.Exit);
    }
}
