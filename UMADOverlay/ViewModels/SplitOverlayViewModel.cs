// Added 2026-10-05. Reuses the upstream Flow mechanic engine without changing its rules. GPL-3.0.
using System.Collections.ObjectModel;
using System.Windows.Media;
using UMADOverlay.Models;
namespace UMADOverlay.ViewModels;
public sealed class HotbarTile
{
 public string Key { get; }
 public string Caption { get; }
 public string Glyph { get; }
 public string? ImagePath { get; }
 public string Description { get; }
 public int Row { get; }
 public int Column { get; }
 public P3ButtonViewModel State { get; }
 public Brush Accent { get; }
 public HotbarTile(string key, string caption, string glyph, string description,
  int row, int column, P3ButtonViewModel state, string accent, string? image = null)
 {
  Key = key; Caption = caption; Glyph = glyph; Description = description;
  Row = row; Column = column; State = state;
  Accent = (Brush)new BrushConverter().ConvertFromString(accent)!;
  ImagePath = image == null ? null : "pack://application:,,,/KefkaSplitOverlay;component/Assets/" + image;
 }
}
public sealed class ResultItem : ViewModelBase
{
 public string Label { get; }
 private string _answer = "—";
 public string Answer { get => _answer; set => Set(ref _answer, value); }
 public ResultItem(string label) => Label = label;
}
public sealed class SplitOverlayViewModel : ViewModelBase
{
 public P3ViewModel Mechanic { get; } = new();
 public ObservableCollection<HotbarTile> Tiles { get; } = new();
 public ObservableCollection<ResultItem> Results { get; } = new();
 public Lang Language { get; private set; }
 public SplitOverlayViewModel(Lang lang)
 {
  Mechanic.PropertyChanged += (_, e) =>
  { if (e.PropertyName?.StartsWith("Ans", StringComparison.Ordinal) == true) UpdateAnswers(); };
  SetLanguage(lang);
 }
 private string L(string zh, string jp, string en) => Language switch
 { Lang.ZH => zh, Lang.JP => jp, _ => en };
 public void SetLanguage(Lang lang)
 {
  Language = lang; Mechanic.UpdateLang(lang); Tiles.Clear(); Results.Clear();
  string truth = L("真", "真", "True"), lie = L("假", "偽", "False");
  void Add(string key, string caption, string glyph, string description, int row, int col,
   P3ButtonViewModel state, string accent, string? image = null) =>
   Tiles.Add(new(key, caption, glyph, description, row, col, state, accent, image));
  const string gc = "#D6B16D", element = "#E18D72", safe = "#89BDD3";
  Add("A", "GC1", "✓", "GC1 · " + truth, 0, 0, Mechanic.BtnA, gc);
  Add("B", "GC1", "✕", "GC1 · " + lie,   0, 1, Mechanic.BtnB, gc);
  Add("J", "GC2", "✓", "GC2 · " + truth, 0, 3, Mechanic.BtnJ, gc);
  Add("K", "GC2", "✕", "GC2 · " + lie,   0, 4, Mechanic.BtnK, gc);
  Add("C", L("早", "早", "Short"), "1", L("GC1 早／GC2 晚", "GC1 早／GC2 遅", "GC1 short / GC2 long"), 1, 0, Mechanic.BtnC, gc);
  Add("D", L("晚", "遅", "Long"), "2", L("GC1 晚／GC2 早", "GC1 遅／GC2 早", "GC1 long / GC2 short"), 1, 1, Mechanic.BtnD, gc);
  Add("E", "GC1", "", L("炸彈來自 GC1", "爆弾は GC1", "Bomb from GC1"), 1, 3, Mechanic.BtnE, gc, "btn_el.png");
  Add("L", "GC2", "", L("炸彈來自 GC2", "爆弾は GC2", "Bomb from GC2"), 1, 4, Mechanic.BtnL, gc, "btn_el.png");
  string ef1 = L("水火1", "水炎1", "E/F 1"), ef2 = L("水火2", "水炎2", "E/F 2");
  Add("F", ef1, "✓", ef1 + " · " + truth, 2, 0, Mechanic.BtnF, element);
  Add("G", ef1, "✕", ef1 + " · " + lie,   2, 1, Mechanic.BtnG, element);
  Add("M", ef2, "✓", ef2 + " · " + truth, 2, 3, Mechanic.BtnM, element);
  Add("N", ef2, "✕", ef2 + " · " + lie,   2, 4, Mechanic.BtnN, element);
  Add("H", L("火", "炎", "Fire"), "", L("火 debuff", "炎デバフ", "Fire debuff"), 3, 0, Mechanic.BtnH, element, "btn_h.png");
  Add("I", L("水", "水", "Water"), "", L("水 debuff", "水デバフ", "Water debuff"), 3, 1, Mechanic.BtnI, element, "btn_i.png");
  string line = L("雷線", "直線", "Line"), cone = L("冰扇", "扇", "Cone");
  Add("O", line, "✓", line + " · " + truth, 3, 3, Mechanic.BtnO, safe);
  Add("P", line, "✕", line + " · " + lie,   3, 4, Mechanic.BtnP, safe);
  Add("Q", cone, "✓", cone + " · " + truth, 4, 3, Mechanic.BtnQ, safe);
  Add("R", cone, "✕", cone + " · " + lie,   4, 4, Mechanic.BtnR, safe);
  foreach (string label in new[] {
   L("1 · 炸彈", "1 · 爆弾", "1 · Bomb"),
   L("2 · 早水雷", "2 · 早 水雷", "2 · Short · water/lightning"),
   L("3 · 早視線", "3 · 早 視線", "3 · Short · gaze"),
   L("4 · 火", "4 · 炎", "4 · Fire"),
   L("5 · 晚水雷", "5 · 遅 水雷", "5 · Long · water/lightning"),
   L("6 · 晚視線", "6 · 遅 視線", "6 · Long · gaze"),
   L("7 · 水", "7 · 水", "7 · Water"),
   L("8 · 魔法放出", "8 · マジックアウト", "8 · Mana release") }) Results.Add(new(label));
  UpdateAnswers();
 }
 private void UpdateAnswers()
 {
  string[] answers = [Mechanic.Ans1, Mechanic.Ans2, Mechanic.Ans3, Mechanic.Ans4,
   Mechanic.Ans5, Mechanic.Ans6, Mechanic.Ans7, Mechanic.Ans8];
  for (int i = 0; i < Results.Count; i++)
   Results[i].Answer = string.IsNullOrEmpty(answers[i]) ? "—" : answers[i];
 }
 public void Reset() => Mechanic.ResetAll();
}
