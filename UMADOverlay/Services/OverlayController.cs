// Added 2026-10-05. One owner for all windows and their shared mechanic state. GPL-3.0.
using System.Windows;
using System.Windows.Threading;
using UMADOverlay.Models;
using UMADOverlay.ViewModels;
using UMADOverlay.Views;

namespace UMADOverlay.Services;

public sealed class OverlayController
{
    private readonly SettingsStore _store;
    private readonly DispatcherTimer _saveTimer = new() { Interval = TimeSpan.FromMilliseconds(600) };
    private readonly TriggerWindow _triggers;
    private readonly ResultsWindow _results;
    private readonly LauncherWindow _launcher;
    private SettingsWindow? _settingsWindow;
    private bool _quitting;
    public UserSettings Settings { get; }
    public SplitOverlayViewModel ViewModel { get; }
    public string? SettingsError => _store.LastError;

    public OverlayController(SettingsStore? store = null)
    {
        _store = store ?? new SettingsStore();
        Settings = _store.Load();
        ViewModel = new(Settings.Language);
        _triggers = new(ViewModel); _results = new(ViewModel); _launcher = new();
        var screen = SystemParameters.WorkArea;
        double left = screen.Left + Math.Max(8, (screen.Width - 540) / 2);
        double top = screen.Top + Math.Max(8, (screen.Height - 420) / 2);
        _triggers.Restore(Settings.Triggers, left, top);
        _results.Restore(Settings.Results, left + 300, top);
        _launcher.Restore(Settings.Launcher, left, top);

        _triggers.AddAction("↺", "Reset mechanic selections", ViewModel.Reset);
        _triggers.AddAction("▣", "Collapse both overlays to an icon", Collapse);
        _triggers.AddAction("⚙", "Settings", ShowSettings);
        _triggers.AddAction("×", "Exit", Exit);
        _results.AddAction("▣", "Collapse both overlays to an icon", Collapse);
        _results.AddAction("⚙", "Settings", ShowSettings);
        _launcher.RestoreRequested += Expand;
        _launcher.ConfigureMenu(Expand, ShowSettings, RecoverPositions, Exit);
        foreach (var window in new OverlayWindow[] { _triggers, _results, _launcher })
        {
            window.PlacementChanged += QueueSave;
            window.ExitRequested += Exit;
            window.ContentRendered += (_, _) => window.KeepOnScreen();
        }
        _saveTimer.Tick += (_, _) => { _saveTimer.Stop(); Save(); };
        ApplyPreferences();
    }

    public void Start()
    {
        if (Settings.Compact) Collapse(); else Expand();
    }

    public void Expand()
    {
        if (_quitting) return;
        Settings.Compact = false;
        _triggers.Show(); _results.Show();
        _triggers.KeepOnScreen(); _results.KeepOnScreen();
        _launcher.Hide();
        QueueSave();
    }

    public void Collapse()
    {
        if (_quitting) return;
        Settings.Compact = true;
        _settingsWindow?.Close();
        _triggers.Hide(); _results.Hide();
        _launcher.Show(); _launcher.KeepOnScreen();
        QueueSave();
    }

    public void SetLanguage(Lang lang)
    {
        Settings.Language = lang;
        ViewModel.SetLanguage(lang);
        _triggers.Rebuild(); _results.Rebuild();
        QueueSave();
    }

    public void ApplyPreferences()
    {
        foreach (var window in new OverlayWindow[] { _triggers, _results })
        {
            window.LayoutLocked = Settings.LayoutLocked;
            window.ApplyOpacity(Settings.BackgroundOpacity);
        }
        QueueSave();
    }

    public void RecoverPositions()
    {
        var screen = SystemParameters.WorkArea;
        double left = screen.Left + 32, top = screen.Top + 64;
        _triggers.Width = 284; _triggers.Height = 318;
        _results.Width = 232; _results.Height = 416;
        _triggers.Left = left; _triggers.Top = top;
        _results.Left = left + 300; _results.Top = top;
        _launcher.Left = left; _launcher.Top = top;
        Expand();
    }

    public void ShowSettings()
    {
        if (_settingsWindow != null) { _settingsWindow.Activate(); return; }
        _settingsWindow = new SettingsWindow(this);
        _settingsWindow.Closed += (_, _) => { _settingsWindow = null; Save(); };
        _settingsWindow.Show();
    }

    private void QueueSave()
    {
        if (_quitting) return;
        _saveTimer.Stop(); _saveTimer.Start();
    }

    public void Save()
    {
        Settings.Triggers = _triggers.Capture();
        Settings.Results = _results.Capture();
        Settings.Launcher = _launcher.Capture();
        _store.Save(Settings);
    }

    public void Exit()
    {
        if (_quitting) return;
        _quitting = true; _saveTimer.Stop(); Save();
        _triggers.AllowClose = _results.AllowClose = _launcher.AllowClose = true;
        Application.Current.Dispatcher.BeginInvoke(new Action(() => Application.Current.Shutdown()));
    }
}
