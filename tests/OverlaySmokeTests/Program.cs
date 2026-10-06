using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using UMADOverlay;
using UMADOverlay.Models;
using UMADOverlay.Services;
using UMADOverlay.Views;

internal static class Program
{
    private static void Check(bool condition,string message) { if (!condition) throw new Exception(message); }
    private static void Pump() => Application.Current.Dispatcher.Invoke(() => { },DispatcherPriority.ApplicationIdle);
    private static void Capture(OverlayWindow window, string name)
    {
        window.UpdateLayout();
        var image = new RenderTargetBitmap((int)Math.Ceiling(window.ActualWidth),
            (int)Math.Ceiling(window.ActualHeight),96,96,System.Windows.Media.PixelFormats.Pbgra32);
        image.Render(window);
        var png = new PngBitmapEncoder(); png.Frames.Add(BitmapFrame.Create(image));
        Directory.CreateDirectory("dist/preview");
        using var stream = File.Create(Path.Combine("dist/preview",name+".png")); png.Save(stream);
    }
    [STAThread]
    private static int Main()
    {
        var app = new App(); app.InitializeComponent();
        string folder=Path.Combine(Path.GetTempPath(),"KefkaSmoke-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var store=new SettingsStore(Path.Combine(folder,"settings.json"));
            var controller=app.StartOverlay(store); Pump();
            var windows=app.Windows.OfType<OverlayWindow>().ToArray();
            var trigger=windows.OfType<TriggerWindow>().Single();
            var results=windows.OfType<ResultsWindow>().Single();
            Check(trigger.IsVisible && results.IsVisible,"Both overlays must open");
            var vm=controller.ViewModel;
            Check(vm.Tiles.Count==18,"All 18 original inputs must be reachable");
            Check(vm.Tiles.Select(t=>(t.Row,t.Column)).Distinct().Count()==18,"No overlapping tiles");
            Check(string.Join("",vm.Tiles.Select(t=>t.Key))=="ABCDEFGIHJKLMNOPQR","Original Flow input order");
            Check(vm.Tiles.Where(t=>t.Key is "A" or "F" or "J" or "M" or "O" or "Q").All(t=>t.Glyph=="●"),"Original blue-circle icons");
            Check(vm.Tiles.Where(t=>t.Key is "B" or "G" or "K" or "N" or "P" or "R").All(t=>t.Glyph=="?"),"Original question-mark icons");
            Check(controller.Settings.BackgroundOpacity==1.0,"Opaque high-contrast default");
            var body=(System.Windows.Controls.Grid)((System.Windows.Controls.Border)trigger.Content).Child;
            var layout=(System.Windows.Controls.Grid)body.Children.OfType<System.Windows.Controls.Viewbox>().Single().Child;
            Check(layout.ColumnDefinitions.Count==3 && layout.RowDefinitions.Count==3,"Three-column grouped layout");
            var bombs=layout.Children.OfType<System.Windows.Controls.Button>().ToArray();
            Check(bombs.Length==2 && bombs.All(b=>System.Windows.Controls.Grid.GetColumn(b)==2),"Separate right-side bomb column");
            Check((string)bombs[0].CommandParameter=="E" && (string)bombs[1].CommandParameter=="L","Bombs retain GC1/GC2 order");
            Check(layout.Children.OfType<System.Windows.Controls.Border>().Count()==6,"Six mechanic groups");
            Check(vm.Tiles.Single(t=>t.Key=="C").Caption=="早(50s)" && vm.Tiles.Single(t=>t.Key=="D").Caption=="晚(1m)","Early/late timing remarks");
            vm.Mechanic.CmdClick.Execute("A");
            vm.Mechanic.CmdClick.Execute("E");
            Check(vm.Results[0].Answer=="不要動","Result window must share the trigger state");
            Capture(trigger,"Input"); Capture(results,"Results");
            double width=trigger.Width;
            controller.Collapse(); Pump();
            var icon=app.Windows.OfType<LauncherWindow>().Single();
            Check(icon.IsVisible && !trigger.IsVisible && !results.IsVisible,"Only icon is visible when compact");
            Check(icon.Width==48 && icon.Height==48,"Launcher must remain icon-sized");
            var iconRoot=(System.Windows.Controls.Grid)((System.Windows.Controls.Border)icon.Content).Child;
            var iconSurface=iconRoot.Children.OfType<System.Windows.Controls.Grid>().Single(g=>System.Windows.Controls.Grid.GetRow(g)==1);
            var iconImage=iconSurface.Children.OfType<System.Windows.Controls.Image>().Single();
            Check(((BitmapImage)iconImage.Source).UriSource.ToString().EndsWith("Assets/launcher_question.png"),"Supplied question-mark icon resource");
            Capture(icon,"Launcher");
            controller.Expand(); Pump();
            Check(trigger.IsVisible && results.IsVisible && !icon.IsVisible,"Both windows restore together");
            Check(trigger.Width==width && vm.Mechanic.BtnA.IsActive,"Restore retains geometry and selections");
            foreach (Lang lang in Enum.GetValues<Lang>()) { controller.SetLanguage(lang); Pump(); }
            Check(vm.Results[0].Answer=="STOP","Language switching retains inputs");
            controller.Settings.LayoutLocked=true; controller.ApplyPreferences();
            Check(trigger.LayoutLocked && results.LayoutLocked,"Lock applies to both overlays");
            controller.Save(); Check(store.LastError==null,"Preferences save successfully");
            foreach (var window in app.Windows.OfType<OverlayWindow>().ToArray()) { window.AllowClose=true; window.Close(); }
            Console.WriteLine("PASS: WPF resources, paired visibility, shared state, language and settings smoke checks.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
        finally { app.Shutdown(); Directory.Delete(folder,true); }
    }
}
