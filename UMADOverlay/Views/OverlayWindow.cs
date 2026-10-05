// Added 2026-10-05. Shared non-activating overlay chrome. GPL-3.0.
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using UMADOverlay.Models;

namespace UMADOverlay.Views;

public class OverlayWindow : Window
{
    private readonly Border _frame;
    private readonly TextBlock _title;
    protected readonly Grid Root = new();
    protected readonly StackPanel Toolbar = new() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
    private bool _layoutLocked;
    public bool LayoutLocked
    {
        get => _layoutLocked;
        set { _layoutLocked = value; ResizeMode = value || IsLauncher ? ResizeMode.NoResize : ResizeMode.CanResize; }
    }
    public bool AllowClose { get; set; }
    public bool IsLauncher { get; }
    public event Action? PlacementChanged;
    public event Action? ExitRequested;

    public OverlayWindow(string title, double width, double height, bool launcher = false)
    {
        Title = title; Width = width; Height = height; IsLauncher = launcher;
        MinWidth = launcher ? 48 : 180; MinHeight = launcher ? 48 : 170;
        MaxWidth = launcher ? 48 : 900; MaxHeight = launcher ? 48 : 1200;
        WindowStyle = WindowStyle.None; AllowsTransparency = true;
        Background = Brushes.Transparent; Topmost = true;
        ShowActivated = false; ShowInTaskbar = false; ResizeMode = launcher ? ResizeMode.NoResize : ResizeMode.CanResize;
        FontFamily = new FontFamily("Segoe UI");
        _frame = new Border { CornerRadius = new CornerRadius(8), BorderThickness = new Thickness(1),
            BorderBrush = Brush("#93A4AC"), Child = Root };
        Content = _frame;
        Root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(launcher ? 0 : 30) });
        Root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        var header = new Grid { Background = Brush("#222C34") };
        _title = new TextBlock { Text = title, Foreground = Brush("#E6D3A1"), FontSize = 10,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8,0,0,0) };
        header.Children.Add(_title); header.Children.Add(Toolbar);
        header.MouseLeftButtonDown += (_, e) =>
        {
            if (!LayoutLocked && e.ButtonState == MouseButtonState.Pressed)
            { DragMove(); e.Handled = true; PlacementChanged?.Invoke(); }
        };
        Root.Children.Add(header);
        SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, -20, GetWindowLong(hwnd, -20) | 0x08000000 | 0x00000080);
            HwndSource.FromHwnd(hwnd)?.AddHook(WndProc);
        };
        LocationChanged += (_, _) => PlacementChanged?.Invoke();
        SizeChanged += (_, _) => PlacementChanged?.Invoke();
        Closing += (_, e) => { if (!AllowClose) { e.Cancel = true; ExitRequested?.Invoke(); } };
        ApplyOpacity(0.8);
    }

    public void SetTitle(string text) { Title = text; _title.Text = text; }
    public void ApplyOpacity(double value) => _frame.Background = new SolidColorBrush(
        Color.FromArgb((byte)(Math.Clamp(value, .2, 1) * 255), 23, 31, 38));
    public static Brush Brush(string hex) => (Brush)new BrushConverter().ConvertFromString(hex)!;

    public Button AddAction(string label, string tooltip, Action action)
    {
        var button = new Button { Content = label, ToolTip = tooltip, Width = 26, Height = 22,
            FontSize = 12, Margin = new Thickness(0,4,3,4), Padding = new Thickness(0) };
        button.Click += (_, e) => { action(); e.Handled = true; };
        Toolbar.Children.Add(button);
        return button;
    }

    protected void SetBody(UIElement element)
    { Grid.SetRow(element, 1); Root.Children.Add(element); }

    public WindowPlacement Capture() => new() { Left = Left, Top = Top, Width = Width, Height = Height };
    public void Restore(WindowPlacement? saved, double left, double top)
    {
        if (saved != null && double.IsFinite(saved.Left) && double.IsFinite(saved.Top)
            && double.IsFinite(saved.Width) && double.IsFinite(saved.Height)
            && saved.Width > 0 && saved.Height > 0)
        {
            Width = Math.Clamp(saved.Width, MinWidth, MaxWidth);
            Height = Math.Clamp(saved.Height, MinHeight, MaxHeight);
            Left = saved.Left; Top = saved.Top;
        }
        else { Left = left; Top = top; }
    }

    // Called after the HWND exists, so conversion uses this window's actual DPI.
    // MonitorFromRect avoids restoring into a gap between disconnected monitors.
    public void KeepOnScreen()
    {
        if (PresentationSource.FromVisual(this) == null) return;
        Point a = PointToScreen(new Point(0,0));
        Point b = PointToScreen(new Point(ActualWidth,ActualHeight));
        RECT rect = new() { Left = (int)a.X, Top = (int)a.Y, Right = (int)b.X, Bottom = (int)b.Y };
        IntPtr monitor = MonitorFromRect(ref rect, 2);
        MONITORINFO info = new() { Size = Marshal.SizeOf<MONITORINFO>() };
        if (!GetMonitorInfo(monitor, ref info)) return;
        Point workStart = PointFromScreen(new Point(info.Work.Left, info.Work.Top));
        Point workEnd = PointFromScreen(new Point(info.Work.Right, info.Work.Bottom));
        double workLeft = Left + workStart.X, workTop = Top + workStart.Y;
        double workWidth = workEnd.X - workStart.X, workHeight = workEnd.Y - workStart.Y;
        Width = Math.Min(Width, Math.Max(MinWidth, workWidth));
        Height = Math.Min(Height, Math.Max(MinHeight, workHeight));
        Left = Math.Clamp(Left, workLeft, Math.Max(workLeft, workLeft + workWidth - Width));
        Top = Math.Clamp(Top, workTop, Math.Max(workTop, workTop + workHeight - Height));
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr w, IntPtr l, ref bool handled)
    {
        if (msg == 0x21) { handled = true; return (IntPtr)3; } // WM_MOUSEACTIVATE / MA_NOACTIVATE
        if (msg == 0x84 && !LayoutLocked && !IsLauncher)
        {
            long packed = l.ToInt64();
            Point p = PointFromScreen(new Point((short)(packed & 0xffff), (short)((packed >> 16) & 0xffff)));
            const int grip = 5;
            bool left = p.X < grip, right = p.X >= ActualWidth - grip;
            bool top = p.Y < grip, bottom = p.Y >= ActualHeight - grip;
            int hit = top && left ? 13 : top && right ? 14 : bottom && left ? 16 : bottom && right ? 17
                : left ? 10 : right ? 11 : top ? 12 : bottom ? 15 : 1;
            if (hit != 1) { handled = true; return (IntPtr)hit; }
        }
        return IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential)] private struct RECT { public int Left, Top, Right, Bottom; }
    [StructLayout(LayoutKind.Sequential)] private struct MONITORINFO { public int Size; public RECT Monitor, Work; public uint Flags; }
    [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr h, int n);
    [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr h, int n, int v);
    [DllImport("user32.dll")] private static extern IntPtr MonitorFromRect(ref RECT rect, uint flags);
    [DllImport("user32.dll", CharSet = CharSet.Auto)] [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref MONITORINFO info);
}
