// Added 2026-10-05. Click restores both overlays; drag moves the icon. GPL-3.0.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UMADOverlay.Views;

public sealed class LauncherWindow : OverlayWindow
{
    private Point _down;
    private bool _pressed;
    private readonly Grid _surface;
    public event Action? RestoreRequested;
    public LauncherWindow() : base("Kefka · restore both overlays", 48, 48, launcher: true)
    {
        _surface = new Grid { Background = Brush("#263441"), Cursor = Cursors.Hand,
            ToolTip = "Click: restore both overlays · Drag: move · Right-click: options" };
        _surface.Children.Add(new TextBlock { Text = "K", FontSize = 27, FontWeight = FontWeights.Bold,
            Foreground = Brush("#F2D69C"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center });
        SetBody(_surface);
        _surface.MouseLeftButtonDown += (_, e) =>
        { _down = PointToScreen(e.GetPosition(this)); _pressed = true; _surface.CaptureMouse(); e.Handled = true; };
        _surface.MouseMove += (_, e) =>
        {
            if (!_pressed || e.LeftButton != MouseButtonState.Pressed) return;
            Point current = PointToScreen(e.GetPosition(this));
            if (Math.Abs(current.X-_down.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(current.Y-_down.Y) < SystemParameters.MinimumVerticalDragDistance) return;
            _pressed = false; _surface.ReleaseMouseCapture();
            DragMove(); KeepOnScreen(); e.Handled = true;
        };
        _surface.MouseLeftButtonUp += (_, e) =>
        {
            bool restore = _pressed; _pressed = false; _surface.ReleaseMouseCapture();
            if (restore) RestoreRequested?.Invoke(); e.Handled = true;
        };
        _surface.LostMouseCapture += (_, _) => _pressed = false;
    }
    public void ConfigureMenu(Action restore, Action settings, Action recover, Action exit)
    {
        var menu = new ContextMenu();
        void Add(string text, Action action) { var item = new MenuItem { Header = text }; item.Click += (_, _) => action(); menu.Items.Add(item); }
        Add("Restore both overlays", restore); Add("Settings", settings);
        Add("Recover window positions", recover); Add("Exit", exit);
        _surface.ContextMenu = menu;
    }
}
