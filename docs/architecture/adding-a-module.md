# Adding a feature module

Example: an OCR module (`snapricot.ocr`). No existing project changes, except the three registration lines below.

## 1. Create the project

```bash
dotnet new classlib -o src/Modules/Snapricot.Modules.Ocr -f net10.0
dotnet sln Snapricot.slnx add --solution-folder src/Modules src/Modules/Snapricot.Modules.Ocr/Snapricot.Modules.Ocr.csproj
```

The csproj needs `<IsAotCompatible>true</IsAotCompatible>`, a reference to `Snapricot.Core`, and UI packages
(e.g. `Avalonia`) without versions (they come from `Directory.Packages.props`). Copy the Screenshot module's csproj.

## 2. Implement the module

```csharp
public sealed class OcrModule : ISnapricotModule
{
    public string Id => "snapricot.ocr";
    public string DisplayName => "Copy text";

    public void Register(IModuleBuilder builder)
    {
        builder.Services.AddSingleton<OcrPageViewModel>();

        builder
            .AddNavItem(new NavItem("snapricot.ocr.page", "Copy text", IconPath: "M...", Order: 300,
                CreateView: sp => new OcrPage { DataContext = sp.GetRequiredService<OcrPageViewModel>() }))
            .AddHotkey(new HotkeyBinding("snapricot.ocr.capture", "Copy text from screen", "Ctrl+Alt+PrtSc",
                (sp, ct) => sp.GetRequiredService<OcrCapture>().RunAsync(ct)));
    }
}
```

- Need something from another module? Subscribe to an event (`IEventBus`) instead of referencing it.
- Need an OS capability? Add an interface to `Snapricot.Core.Platform` and implement it in each platform project.
- `Register` must be cheap: no I/O, no windows, no heavy objects. Create things lazily.

## 3. Register it (the only edits outside the new project)

1. `src/Snapricot.App/Snapricot.App.csproj`: add a `ProjectReference`.
2. `src/Snapricot.App/Composition/InstalledModules.cs`: add `new OcrModule(),`.
3. `tests/Snapricot.Architecture.Tests`: add a `ProjectReference`, plus `typeof(OcrModule).Assembly.GetName().Name!`
   to `ModuleAssemblies`.

## 4. Checklist

- [ ] `dotnet build` with no warnings; `dotnet test` green (including the architecture tests)
- [ ] All `.axaml` files use `x:DataType` (compiled bindings)
- [ ] Logging uses `[LoggerMessage]`
- [ ] Startup budget unchanged (CI's startup measurement)
- [ ] Docs: add an ADR if you made a significant decision
