// Modified 2026-10-05: paired overlays and a compact launcher. GPL-3.0.
using System.Windows;
using UMADOverlay.Services;
namespace UMADOverlay;
public partial class App : Application
{
 private OverlayController? _controller;
 protected override void OnStartup(StartupEventArgs e)
 {
  base.OnStartup(e);
  StartOverlay();
 }
 // A single initialization path also lets the Windows harness use temporary
 // settings without racing WPF's queued startup callback.
 public OverlayController StartOverlay(SettingsStore? settingsStore = null)
 {
  if (_controller != null) return _controller;
  _controller = new OverlayController(settingsStore);
  _controller.Start();
  return _controller;
 }
 protected override void OnExit(ExitEventArgs e)
 {
  _controller?.Save();
  base.OnExit(e);
 }
}
