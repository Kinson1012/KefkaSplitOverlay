// Added 2026-10-05. GPL-3.0.
using System.IO;
using System.Text.Json;
using UMADOverlay.Models;
namespace UMADOverlay.Services;
public sealed class SettingsStore
{
 public string FilePath { get; }
 public SettingsStore(string? filePath = null) => FilePath = filePath ?? Path.Combine(
  Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
  "KefkaSplitOverlay", "settings.json");
 public string? LastError { get; private set; }
 public UserSettings Load()
 {
  try
  {
   if (!File.Exists(FilePath)) return new() { AppearanceVersion=1, InputLayoutVersion=1 };
   var result = JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(FilePath)) ?? new();
   if (!Enum.IsDefined(result.Language)) result.Language = Lang.ZH;
   result.BackgroundOpacity = double.IsFinite(result.BackgroundOpacity)
    ? Math.Clamp(result.BackgroundOpacity, 0.20, 1.0) : 1.0;
   // Upgrade the previous translucent theme once; keep the user's other preferences.
   if (result.AppearanceVersion<1) { result.BackgroundOpacity=1.0; result.AppearanceVersion=1; }
   // Apply the new compact-wide proportions once, retaining the saved position.
   if (result.InputLayoutVersion<1)
   {
    if (result.Triggers!=null) { result.Triggers.Width=384; result.Triggers.Height=262; }
    result.InputLayoutVersion=1;
   }
   return result;
  }
  catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
  {
   LastError = "Settings could not be loaded; defaults are in use. " + ex.Message;
   return new();
  }
 }
 public void Save(UserSettings settings)
 {
  try
  {
   Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
   string temp = FilePath + ".tmp";
   File.WriteAllText(temp, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
   File.Move(temp, FilePath, overwrite: true);
   LastError = null;
  }
  catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
  { LastError = "Settings could not be saved. " + ex.Message; }
 }
}
