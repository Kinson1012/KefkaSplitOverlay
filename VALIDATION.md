# Validation record

2026-10-05

- PASS: Unchanged upstream: LICENSE
- PASS: Unchanged upstream: UMADOverlay/ViewModels/P3ViewModel.cs
- PASS: Unchanged upstream: UMADOverlay/ViewModels/ViewModelBase.cs
- PASS: Unchanged upstream: UMADOverlay/Models/I18n.cs
- PASS: Unchanged upstream: UMADOverlay/Models/GimmickData.cs
- PASS: XML: App.xaml and all three csproj files parse
- PASS: All 18 command keys map to matching state, with unique cells and a blank centre column
- PASS: All three PNGs decode and are referenced by hotbar tiles
- PASS: Translation dictionaries have matching 64-key sets
- PASS: Standalone source copy has no upstream .git directory or remote

Not executed: .NET compilation, C# regression tests, WPF smoke tests, and Windows/gameplay interaction. A .NET SDK/Windows desktop was unavailable. The included GitHub workflow runs the build and C# checks on Windows.
