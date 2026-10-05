// Added 2026-10-05. GPL-3.0.
namespace UMADOverlay.Models;
public sealed class WindowPlacement
{
 public double Left { get; set; }
 public double Top { get; set; }
 public double Width { get; set; }
 public double Height { get; set; }
}
public sealed class UserSettings
{
 public Lang Language { get; set; } = Lang.ZH;
 public double BackgroundOpacity { get; set; } = 0.80;
 public bool LayoutLocked { get; set; }
 public bool Compact { get; set; }
 public WindowPlacement? Triggers { get; set; }
 public WindowPlacement? Results { get; set; }
 public WindowPlacement? Launcher { get; set; }
}
