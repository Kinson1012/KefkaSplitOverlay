using System.IO;
using System.Windows;
using System.Windows.Threading;
using UMADOverlay;
using UMADOverlay.Models;
using UMADOverlay.Services;
using UMADOverlay.Views;

internal static class Program
{
    private static void Check(bool condition,string message) { if (!condition) throw new Exception(message); }
    private static void Pump() => Application.Current.Dispatcher.Invoke(() => { },DispatcherPriority.ApplicationIdle);
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
            Check(vm.Tiles.All(t=>t.Column!=2),"Centre column must remain empty");
            vm.Mechanic.CmdClick.Execute("A");
            vm.Mechanic.CmdClick.Execute("E");
            Check(vm.Results[0].Answer=="不要動","Result window must share the trigger state");
            double width=trigger.Width;
            controller.Collapse(); Pump();
            var icon=app.Windows.OfType<LauncherWindow>().Single();
            Check(icon.IsVisible && !trigger.IsVisible && !results.IsVisible,"Only icon is visible when compact");
            Check(icon.Width==48 && icon.Height==48,"Launcher must remain icon-sized");
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
