// Modified 2026-10-06: restore upstream Flow order, text and icons in a compact panel. GPL-3.0.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UMADOverlay.ViewModels;
namespace UMADOverlay.Views;
public sealed class TriggerWindow : OverlayWindow
{
 private readonly Grid _flow = new() { Width = 262, Height = 276 };
 private readonly SplitOverlayViewModel _vm;
 public TriggerWindow(SplitOverlayViewModel vm) : base("KEFKA · INPUT", 284, 318)
 {
  _vm=vm; DataContext=vm; MinWidth=204; MinHeight=228;
  _flow.ColumnDefinitions.Add(new ColumnDefinition());
  _flow.ColumnDefinitions.Add(new ColumnDefinition());
  int[] headings=[0,5,9,13,16,18], dividers=[4,8,12,15];
  for (int i=0;i<20;i++) _flow.RowDefinitions.Add(new RowDefinition {
   Height=new GridLength(headings.Contains(i)?14:dividers.Contains(i)?2:18.4) });
  SetBody(new Viewbox { Stretch=Stretch.Uniform, Margin=new Thickness(8,5,8,5), Child=_flow });
  Rebuild();
 }
 public void Rebuild()
 {
  _flow.Children.Clear();
  foreach (var heading in _vm.FlowHeaders)
  {
   var label=new TextBlock { Text=heading.Value, FontSize=11, FontWeight=FontWeights.Bold,
    Foreground=Brush("#FFFFFF"), TextAlignment=TextAlignment.Center, VerticalAlignment=VerticalAlignment.Center };
   Grid.SetRow(label,heading.Key); Grid.SetColumnSpan(label,2); _flow.Children.Add(label);
  }
  foreach (int row in new[]{4,8,12,15})
  {
   var line=new Border { Background=Brush("#FFFFFF"), Height=1, Margin=new Thickness(0) };
   Grid.SetRow(line,row); Grid.SetColumnSpan(line,2); _flow.Children.Add(line);
  }
  foreach (var tile in _vm.Tiles)
  {
   UIElement content;
   if (tile.ImagePath!=null)
    content=new Image { Source=new BitmapImage(new Uri(tile.ImagePath)), Height=16, Stretch=Stretch.Uniform };
   else if (tile.Glyph=="●") content=new Ellipse { Width=14, Height=14, Fill=Brush("#4DADFF") };
   else content=new TextBlock { Text=tile.Glyph=="?"?"?":tile.Caption, FontSize=14,
    FontWeight=FontWeights.Bold, Foreground=tile.Glyph=="?"?Brush("#FF4D4D"):Brush("#FFFFFF"),
    TextAlignment=TextAlignment.Center, VerticalAlignment=VerticalAlignment.Center };
   var style=new Style(typeof(Button),(Style)Application.Current.FindResource(typeof(Button)));
   style.Setters.Add(new Setter(BackgroundProperty,Brush("#141414")));
   style.Setters.Add(new Setter(BorderBrushProperty,Brush("#CCCCCC")));
   var active=new DataTrigger { Binding=new Binding("State.IsActive"), Value=true };
   active.Setters.Add(new Setter(BackgroundProperty,Brush("#003C70")));
   active.Setters.Add(new Setter(BorderBrushProperty,Brush("#FFFF00")));
   active.Setters.Add(new Setter(BorderThicknessProperty,new Thickness(2)));
   style.Triggers.Add(active);
   var button=new Button { Margin=new Thickness(1,0,1,1), Padding=new Thickness(0), Content=content,
    DataContext=tile, Style=style, ToolTip=tile.Description,
    Command=_vm.Mechanic.CmdClick, CommandParameter=tile.Key };
   System.Windows.Automation.AutomationProperties.SetName(button,tile.Description);
   Grid.SetRow(button,tile.Row); Grid.SetColumn(button,tile.Column);
   if (tile.Key is "E" or "L") Grid.SetColumnSpan(button,2);
   _flow.Children.Add(button);
  }
 }
}
