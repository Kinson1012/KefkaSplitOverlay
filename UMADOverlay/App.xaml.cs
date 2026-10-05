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
  _controller = new OverlayController();
  _controller.Start();
 }
 protected override void OnExit(ExitEventArgs e)
 {
  _controller?.Save();
  base.OnExit(e);
 }
}
