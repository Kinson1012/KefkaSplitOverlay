// Modified 2026-10-06: opaque high-contrast rows and upstream Flow labels. GPL-3.0.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using UMADOverlay.ViewModels;
namespace UMADOverlay.Views;
public sealed class ResultsWindow : OverlayWindow
{
 private readonly StackPanel _rows=new() { Width=210 };
 private readonly SplitOverlayViewModel _vm;
 public ResultsWindow(SplitOverlayViewModel vm) : base("KEFKA · RESULTS",232,416)
 {
  _vm=vm; DataContext=vm; MinWidth=180; MinHeight=270;
  SetBody(new Viewbox { Stretch=Stretch.Uniform, Margin=new Thickness(8), Child=_rows });
  Rebuild();
 }
 public void Rebuild()
 {
  _rows.Children.Clear();
  foreach (var item in _vm.Results)
  {
   if (item.Header!=null) _rows.Children.Add(new TextBlock { Text=item.Header,
    Foreground=Brush("#FFFFFF"), FontSize=13, FontWeight=FontWeights.Bold, Margin=new Thickness(0,4,0,3) });
   var grid=new Grid { Margin=new Thickness(5,4,5,4) };
   grid.ColumnDefinitions.Add(new ColumnDefinition { Width=new GridLength(26) });
   grid.ColumnDefinitions.Add(new ColumnDefinition());
   grid.Children.Add(new TextBlock { Text=item.Label, FontSize=14, FontWeight=FontWeights.Bold,
    Foreground=Brush("#FFFFFF"), VerticalAlignment=VerticalAlignment.Center });
   var answer=new TextBlock { FontSize=20, FontWeight=FontWeights.Bold,
    Foreground=Brush("#FFFF00"), TextWrapping=TextWrapping.Wrap, VerticalAlignment=VerticalAlignment.Center };
   answer.SetBinding(TextBlock.TextProperty,new Binding(nameof(ResultItem.Answer)) { Source=item });
   Grid.SetColumn(answer,1); grid.Children.Add(answer);
   _rows.Children.Add(new Border { Child=grid, MinHeight=36, Margin=new Thickness(0,0,0,2),
    BorderThickness=new Thickness(1), BorderBrush=Brush("#777777"),
    Background=Brush("#000000"), CornerRadius=new CornerRadius(3) });
  }
 }
}
