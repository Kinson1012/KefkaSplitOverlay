// Added 2026-10-05. Independently positioned result-only overlay. GPL-3.0.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using UMADOverlay.ViewModels;

namespace UMADOverlay.Views;

public sealed class ResultsWindow : OverlayWindow
{
    private readonly StackPanel _rows = new() { Width = 210 };
    private readonly SplitOverlayViewModel _vm;
    public ResultsWindow(SplitOverlayViewModel vm) : base("KEFKA · RESULTS", 232, 416)
    {
        _vm = vm; DataContext = vm;
        MinWidth = 180; MinHeight = 270;
        SetBody(new Viewbox { Stretch = Stretch.Uniform, Margin = new Thickness(8), Child = _rows });
        Rebuild();
    }
    public void Rebuild()
    {
        _rows.Children.Clear();
        foreach (var item in _vm.Results)
        {
            var text = new StackPanel { Margin = new Thickness(8,4,8,4) };
            text.Children.Add(new TextBlock { Text = item.Label, Foreground = Brush("#AAB9C3"), FontSize = 9 });
            var answer = new TextBlock { FontSize = 17, FontWeight = FontWeights.SemiBold,
                Foreground = Brush("#FFE0A0"), TextWrapping = TextWrapping.Wrap };
            answer.SetBinding(TextBlock.TextProperty, new Binding(nameof(ResultItem.Answer)) { Source = item });
            text.Children.Add(answer);
            _rows.Children.Add(new Border { Child = text, MinHeight = 43, Margin = new Thickness(0,0,0,3),
                BorderThickness = new Thickness(2,0,0,0), BorderBrush = Brush("#72899A"),
                Background = Brush("#B3222C34"), CornerRadius = new CornerRadius(4) });
        }
    }
}
