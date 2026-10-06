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
 public string? Header { get; }
 private string _answer = "—";
 public string Answer { get => _answer; set => Set(ref _answer, value); }
 public ResultItem(string label, string? header = null) { Label = label; Header = header; }
}
public sealed class SplitOverlayViewModel : ViewModelBase
{
 public P3ViewModel Mechanic { get; } = new();
 public ObservableCollection<HotbarTile> Tiles { get; } = new();
 public ObservableCollection<ResultItem> Results { get; } = new();
 public Lang Language { get; private set; }
 public Dictionary<int,string> FlowHeaders { get; } = new();
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
  void Add(string key, string caption, string glyph, string description, int row, int col,
   P3ButtonViewModel state, string accent, string? image = null) =>
   Tiles.Add(new(key, caption, glyph, description, row, col, state, accent, image));
  const string accent = "#FFFFFF";
  string truth=L("真", "真", "True"), lie=L("假", "偽", "False");
  FlowHeaders.Clear();
  FlowHeaders[0]=Mechanic.SecGC1; FlowHeaders[5]=Mechanic.SecWaterFire1;
  FlowHeaders[9]=Mechanic.SecGC2; FlowHeaders[13]=Mechanic.SecWaterFire2;
  FlowHeaders[16]=Mechanic.SecThunder; FlowHeaders[18]=Mechanic.SecIce;
  Add("A", "", "●", Mechanic.SecGC1+" · "+truth, 1, 0, Mechanic.BtnA, accent);
  Add("B", "", "?", Mechanic.SecGC1+" · "+lie, 1, 1, Mechanic.BtnB, accent);
  Add("C", Mechanic.BtnCLabel+"(50s)", "", Mechanic.SecGC1+" · "+Mechanic.BtnCLabel+" (50s)", 2, 0, Mechanic.BtnC, accent);
  Add("D", Mechanic.BtnDLabel+"(1m)", "", Mechanic.SecGC1+" · "+Mechanic.BtnDLabel+" (1m)", 2, 1, Mechanic.BtnD, accent);
  Add("E", "", "", Mechanic.SecBomb+" · "+Mechanic.SecGC1, 3, 0, Mechanic.BtnE, accent, "btn_el.png");
  Add("F", "", "●", Mechanic.SecWaterFire1+" · "+truth, 6, 0, Mechanic.BtnF, accent);
  Add("G", "", "?", Mechanic.SecWaterFire1+" · "+lie, 6, 1, Mechanic.BtnG, accent);
  // Water is on the left and fire on the right, exactly as in upstream Flow.
  Add("I", "", "", L("水 debuff", "水デバフ", "Water debuff"), 7, 0, Mechanic.BtnI, accent, "btn_i.png");
  Add("H", "", "", L("火 debuff", "炎デバフ", "Fire debuff"), 7, 1, Mechanic.BtnH, accent, "btn_h.png");
  Add("J", "", "●", Mechanic.SecGC2+" · "+truth, 10, 0, Mechanic.BtnJ, accent);
  Add("K", "", "?", Mechanic.SecGC2+" · "+lie, 10, 1, Mechanic.BtnK, accent);
  Add("L", "", "", Mechanic.SecBomb+" · "+Mechanic.SecGC2, 11, 0, Mechanic.BtnL, accent, "btn_el.png");
  Add("M", "", "●", Mechanic.SecWaterFire2+" · "+truth, 14, 0, Mechanic.BtnM, accent);
  Add("N", "", "?", Mechanic.SecWaterFire2+" · "+lie, 14, 1, Mechanic.BtnN, accent);
  Add("O", "", "●", Mechanic.SecThunder+" · "+truth, 17, 0, Mechanic.BtnO, accent);
  Add("P", "", "?", Mechanic.SecThunder+" · "+lie, 17, 1, Mechanic.BtnP, accent);
  Add("Q", "", "●", Mechanic.SecIce+" · "+truth, 19, 0, Mechanic.BtnQ, accent);
  Add("R", "", "?", Mechanic.SecIce+" · "+lie, 19, 1, Mechanic.BtnR, accent);
  for (int i=1;i<=8;i++)
  {
   string label=i==4 ? Mechanic.Num4Label : i==7 ? Mechanic.Num7Label : i+".";
   string? header=i switch {1=>Mechanic.SecBomb,2=>Mechanic.SecEarly,5=>Mechanic.SecLate,8=>Mechanic.SecMagic,_=>null};
   Results.Add(new(label,header));
  }
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
