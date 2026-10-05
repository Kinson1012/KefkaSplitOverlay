// Regression tests for retained mechanic rules and new persistence. No test packages required.
using UMADOverlay.Models;
using UMADOverlay.Services;
using UMADOverlay.ViewModels;

static void Equal<T>(T expected, T actual, string label)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception($"{label}: expected {expected}, got {actual}");
}

var vm = new P3ViewModel();
vm.UpdateLang(Lang.EN);
// All choices in seven three-state mutex groups plus the two binary groups.
string[][] groups = [["A","B"],["C","D"],["F","G"],["H","I"],["J","K"],["E","L"],["M","N"]];
int cases = 0;
for (int mask = 0; mask < 2187; mask++)
for (int line = 0; line < 2; line++)
for (int cone = 0; cone < 2; cone++)
{
    vm.ResetAll();
    int[] choice = new int[7]; int remaining = mask;
    for (int g=0; g<7; g++)
    {
        choice[g] = remaining % 3; remaining /= 3;
        if (choice[g] != 0) vm.CmdClick.Execute(groups[g][choice[g]-1]);
    }
    if (line != 0) vm.CmdClick.Execute("P");
    if (cone != 0) vm.CmdClick.Execute("R");
    int gc1=choice[0], timing=choice[1], ef1=choice[2], element=choice[3], gc2=choice[4], bomb=choice[5], ef2=choice[6];
    string Gaze(int gc) => gc == 0 ? "" : gc == 1 ? "Look Away" : "Look At";
    string Spread(int gc) => gc == 0 ? "" : gc == 1 ? "Lightning Out" : "Water Out";
    int bombTruth = bomb == 1 ? gc1 : bomb == 2 ? gc2 : 0;
    Equal(bombTruth == 0 ? "" : bombTruth == 1 ? "STOP" : "MOVE", vm.Ans1, "Bomb");
    Equal(timing == 0 ? "" : Spread(timing == 1 ? gc1 : gc2), vm.Ans2, "Short spread");
    Equal(Gaze(gc1), vm.Ans3, "GC1 gaze");
    int fireTruth = element == 1 ? ef1 : element == 2 ? ef2 : 0;
    int waterTruth = element == 2 ? ef1 : element == 1 ? ef2 : 0;
    Equal(fireTruth == 0 ? "" : fireTruth == 1 ? "Out" : "In", vm.Ans4, "Fire");
    Equal(timing == 0 ? "" : Spread(timing == 1 ? gc2 : gc1), vm.Ans5, "Long spread");
    Equal(Gaze(gc2), vm.Ans6, "GC2 gaze");
    Equal(waterTruth == 0 ? "" : waterTruth == 1 ? "In" : "Out", vm.Ans7, "Water");
    string[,] safe = {{"Avoid Both","Stand in Cone"},{"Stand in Line","Both"}};
    Equal(safe[line,cone], vm.Ans8, "Safe zones");
    cases++;
}
vm.ResetAll(); vm.CmdClick.Execute("A"); vm.CmdClick.Execute("B");
Equal(false,vm.BtnA.IsActive,"Mutex clears peer");
vm.CmdClick.Execute("B"); Equal("",vm.Ans3,"Second click clears normal selection");
vm.CmdClick.Execute("P"); vm.CmdClick.Execute("P");
Equal(true,vm.BtnO.IsActive,"Line falls back to true");
vm.CmdClick.Execute("A"); vm.UpdateLang(Lang.JP); Equal("見ない",vm.Ans3,"Japanese");
vm.UpdateLang(Lang.ZH); Equal("不要看",vm.Ans3,"Chinese");
vm.UpdateLang(Lang.EN); Equal("Look Away",vm.Ans3,"English");
foreach (var lang in Enum.GetValues<Lang>())
    foreach (var key in I18n.Data[Lang.EN].Keys)
        Equal(true,I18n.Data[lang].ContainsKey(key),$"Translation {lang}/{key}");

string folder = Path.Combine(Path.GetTempPath(),"KefkaSplitTests-"+Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(folder);
try
{
    var store = new SettingsStore(Path.Combine(folder,"settings.json"));
    var settings = new UserSettings { Language=Lang.EN, Compact=true, BackgroundOpacity=.65, AppearanceVersion=1, InputLayoutVersion=1,
        Triggers = new WindowPlacement { Left=-600, Top=80, Width=284, Height=318 } };
    store.Save(settings); Equal<string?>(null,store.LastError,"Save success");
    var loaded=store.Load(); Equal(Lang.EN,loaded.Language,"Stored language");
    Equal(true,loaded.Compact,"Stored compact state"); Equal(-600d,loaded.Triggers!.Left,"Negative monitor position");
    File.WriteAllText(store.FilePath,"not json");
    Equal(Lang.ZH,store.Load().Language,"Corrupt file fallback");
    File.WriteAllText(store.FilePath,"{\"Language\":99,\"BackgroundOpacity\":-5,\"AppearanceVersion\":1}");
    loaded=store.Load(); Equal(Lang.ZH,loaded.Language,"Invalid language fallback");
    Equal(.2,loaded.BackgroundOpacity,"Opacity clamp");
    File.WriteAllText(store.FilePath,"{\"BackgroundOpacity\":0.8,\"Compact\":true}");
    loaded=store.Load(); Equal(1d,loaded.BackgroundOpacity,"Upgrade translucent theme");
    Equal(true,loaded.Compact,"Upgrade retains compact preference");
    Equal(1,loaded.AppearanceVersion,"Upgrade only applies once");
    File.WriteAllText(store.FilePath,"{\"Triggers\":{\"Left\":123,\"Top\":456,\"Width\":284,\"Height\":318}}");
    loaded=store.Load(); Equal(384d,loaded.Triggers!.Width,"Upgrade input proportions");
    Equal(262d,loaded.Triggers.Height,"Upgrade input height");
    Equal(123d,loaded.Triggers.Left,"Retain input position");
    loaded.Triggers.Width=420; store.Save(loaded);
    Equal(420d,store.Load().Triggers!.Width,"Do not repeat layout migration");
}
finally { Directory.Delete(folder,true); }
Console.WriteLine($"PASS: {cases} mechanic combinations, toggles, translations and persistence.");
