// Added 2026-10-05. Five-column square hotbar, with a centre gap. GPL-3.0.
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UMADOverlay.ViewModels;

namespace UMADOverlay.Views;

public sealed class TriggerWindow : OverlayWindow
{
    private readonly Grid _hotbar = new() { Width = 262, Height = 276, Margin = new Thickness(4) };
    private readonly SplitOverlayViewModel _vm;
    public TriggerWindow(SplitOverlayViewModel vm) : base("KEFKA · INPUT", 284, 318)
    {
        _vm = vm; DataContext = vm;
        MinWidth = 204; MinHeight = 228;
        for (int i = 0; i < 5; i++)
        {
            _hotbar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(52.4) });
            _hotbar.RowDefinitions.Add(new RowDefinition { Height = new GridLength(55.2) });
        }
        SetBody(new Viewbox { Stretch = Stretch.Uniform, Margin = new Thickness(5), Child = _hotbar });
        Rebuild();
    }

    public void Rebuild()
    {
        _hotbar.Children.Clear();
        // Empty hotbar cells keep the compact silhouette without adding fake inputs.
        for (int row = 0; row < 5; row++) for (int col = 0; col < 5; col++)
        {
            if (col == 2) continue;
            var slot = new Border { Width = 48, Height = 48, CornerRadius = new CornerRadius(5),
                Background = Brush("#222C34"), BorderBrush = Brush("#46555F"), BorderThickness = new Thickness(1), Opacity = 0.5 };
            Grid.SetRow(slot,row); Grid.SetColumn(slot,col); _hotbar.Children.Add(slot);
        }
        foreach (var tile in _vm.Tiles)
        {
            var content = new StackPanel();
            if (tile.ImagePath != null)
                content.Children.Add(new Image { Source = new BitmapImage(new Uri(tile.ImagePath)), Height = 26, Stretch = Stretch.Uniform });
            else content.Children.Add(new TextBlock { Text = tile.Glyph, FontSize = 23, FontWeight = FontWeights.Bold,
                Height = 28, TextAlignment = TextAlignment.Center,
                Foreground = tile.Glyph == "✕" ? Brush("#FFA3A3") : Brush("#F9E5AF") });
            content.Children.Add(new TextBlock { Text = tile.Caption, FontSize = 9, TextAlignment = TextAlignment.Center });
            var style = new Style(typeof(Button), (Style)Application.Current.FindResource(typeof(Button)));
            style.Setters.Add(new Setter(BackgroundProperty, Brush("#35424C")));
            style.Setters.Add(new Setter(BorderBrushProperty, tile.Accent));
            var active = new DataTrigger { Binding = new Binding("State.IsActive"), Value = true };
            active.Setters.Add(new Setter(BackgroundProperty, Brush("#536D87")));
            active.Setters.Add(new Setter(BorderBrushProperty, Brush("#FFE4A3")));
            active.Setters.Add(new Setter(BorderThicknessProperty, new Thickness(2)));
            style.Triggers.Add(active);
            var button = new Button { Width = 48, Height = 48, Padding = new Thickness(1), Content = content,
                DataContext = tile, Style = style, ToolTip = tile.Description,
                Command = _vm.Mechanic.CmdClick, CommandParameter = tile.Key };
            System.Windows.Automation.AutomationProperties.SetName(button, tile.Description);
            Grid.SetRow(button,tile.Row); Grid.SetColumn(button,tile.Column); _hotbar.Children.Add(button);
        }
    }
}
