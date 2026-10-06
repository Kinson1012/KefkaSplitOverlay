// Modified 2026-10-06: grouped layout from the user's sketch. Original inputs/icons retained. GPL-3.0.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UMADOverlay.ViewModels;
namespace UMADOverlay.Views;
public sealed class TriggerWindow : OverlayWindow
{
 public const double DefaultWidth=384, DefaultHeight=262;
 private readonly Grid _flow=new() { Width=360, Height=216 };
 private readonly SplitOverlayViewModel _vm;
 public TriggerWindow(SplitOverlayViewModel vm) : base("KEFKA · INPUT",DefaultWidth,DefaultHeight)
 {
  _vm=vm; DataContext=vm; MinWidth=270; MinHeight=190;
  foreach (double width in new[]{144d,144d,72d})
   _flow.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(width) });
  foreach (double height in new[]{76d,76d,64d})
   _flow.RowDefinitions.Add(new RowDefinition { Height=new GridLength(height) });
  SetBody(new Viewbox { Stretch=Stretch.Uniform, Margin=new Thickness(8,5,8,5), Child=_flow });
  Rebuild();
 }
 private Button MakeButton(string key)
 {
  var tile=_vm.Tiles.Single(t=>t.Key==key);
  UIElement content;
  if (tile.ImagePath!=null)
   content=new Image { Source=new BitmapImage(new Uri(tile.ImagePath)),
    Height=key is "E" or "L"?40:24, Stretch=Stretch.Uniform };
  else if (tile.Glyph=="●") content=new Ellipse { Width=22, Height=22, Fill=Brush("#4DADFF") };
  else content=new TextBlock { Text=tile.Glyph=="?"?"?":tile.Caption,
   FontSize=tile.Glyph=="?"?25:16, FontWeight=FontWeights.Bold,
   Foreground=tile.Glyph=="?"?Brush("#FF4D4D"):Brush("#FFFFFF"),
   TextAlignment=TextAlignment.Center, VerticalAlignment=VerticalAlignment.Center };
  if (key is "C" or "D") content=new Viewbox { Stretch=Stretch.Uniform, StretchDirection=StretchDirection.DownOnly, Margin=new Thickness(2,0,2,0), Child=content };
  var style=new Style(typeof(Button),(Style)Application.Current.FindResource(typeof(Button)));
  style.Setters.Add(new Setter(BackgroundProperty,Brush("#141414")));
  style.Setters.Add(new Setter(BorderBrushProperty,Brush("#CCCCCC")));
  var active=new DataTrigger { Binding=new Binding("State.IsActive"), Value=true };
  active.Setters.Add(new Setter(BackgroundProperty,Brush("#003C70")));
  active.Setters.Add(new Setter(BorderBrushProperty,Brush("#FFFF00")));
  active.Setters.Add(new Setter(BorderThicknessProperty,new Thickness(2)));
  style.Triggers.Add(active);
  var button=new Button { Margin=new Thickness(1), Padding=new Thickness(0), Content=content,
   DataContext=tile, Style=style, ToolTip=tile.Description,
   Command=_vm.Mechanic.CmdClick, CommandParameter=tile.Key };
  System.Windows.Automation.AutomationProperties.SetName(button,tile.Description);
  return button;
 }
 private void AddGroup(string title,int row,int column,params string[][] inputs)
 {
  var panel=new Grid { Margin=new Thickness(3) };
  panel.RowDefinitions.Add(new RowDefinition { Height=new GridLength(16) });
  var heading=new TextBlock { Text=title, FontSize=13, FontWeight=FontWeights.Bold,
   Foreground=Brush("#FFFFFF"), TextAlignment=TextAlignment.Center };
  panel.Children.Add(heading);
  for (int i=0;i<inputs.Length;i++)
  {
   panel.RowDefinitions.Add(new RowDefinition());
   var buttons=new UniformGrid { Rows=1, Columns=inputs[i].Length };
   foreach (string key in inputs[i]) buttons.Children.Add(MakeButton(key));
   Grid.SetRow(buttons,i+1); panel.Children.Add(buttons);
  }
  var frame=new Border { Child=panel, BorderBrush=Brush("#FFFFFF"),
   BorderThickness=new Thickness(1), Background=Brush("#000000"), Margin=new Thickness(1) };
  Grid.SetRow(frame,row); Grid.SetColumn(frame,column); _flow.Children.Add(frame);
 }
 public void Rebuild()
 {
  _flow.Children.Clear();
  AddGroup(_vm.Mechanic.SecGC1,0,0,new[]{"A","B"},new[]{"C","D"});
  AddGroup(_vm.Mechanic.SecWaterFire1,0,1,new[]{"F","G"},new[]{"I","H"});
  AddGroup(_vm.Mechanic.SecGC2,1,0,new[]{"J","K"});
  AddGroup(_vm.Mechanic.SecWaterFire2,1,1,new[]{"M","N"});
  AddGroup(_vm.Mechanic.SecThunder,2,0,new[]{"O","P"});
  AddGroup(_vm.Mechanic.SecIce,2,1,new[]{"Q","R"});
  foreach (var (key,row) in new[]{("E",0),("L",1)})
  {
   var bomb=MakeButton(key); Grid.SetRow(bomb,row); Grid.SetColumn(bomb,2);
   _flow.Children.Add(bomb);
  }
 }
}
