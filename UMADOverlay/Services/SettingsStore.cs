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
   if (!File.Exists(FilePath)) return new();
   var result = JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(FilePath)) ?? new();
   if (!Enum.IsDefined(result.Language)) result.Language = Lang.ZH;
   result.BackgroundOpacity = double.IsFinite(result.BackgroundOpacity)
    ? Math.Clamp(result.BackgroundOpacity, 0.20, 1.0) : 0.80;
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
