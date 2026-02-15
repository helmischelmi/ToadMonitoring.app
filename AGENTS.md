# Repository Guidelines

## Project Structure & Module Organization
This repository is a .NET 8 desktop application organized as a layered solution in `ToadCapture.Wpf.sln`.

- `ToadCapture.Wpf/`: WPF UI (`Views/`, `ViewModels/`, `Converters/`, `Services/`)
- `ToadCapture.Core/`: domain models and validation rules (`Models/`, `Rules/`)
- `ToadCapture.Persistence/`: SQLite and journaling infrastructure (`Db/`, `Journaling/`)
- `ToadCapture.ImportExport/`: import/export logic (ClosedXML-based)
- `ToadMonitoring.WPF/`: secondary WPF project shell (minimal)

Build outputs are under `bin/` and `obj/`; do not edit generated files.

## Build, Test, and Development Commands
Run commands from repository root.

- `dotnet restore ToadCapture.Wpf.sln`: restore NuGet packages
- `$env:DOTNET_CLI_HOME='D:\Projects\Karch\ToadMonitoring.app\.dotnet'; dotnet build ToadCapture.Wpf.sln -c Debug --no-restore -v minimal`: local Debug build (verified)
- `dotnet run --project ToadCapture.Wpf/ToadCapture.Wpf.csproj -c Debug`: launch the main WPF app
- `dotnet build ToadCapture.Wpf.sln -c Release`: production build

## Coding Style & Naming Conventions
- Language: C# with `Nullable` and `ImplicitUsings` enabled.
- Indentation: 4 spaces; UTF-8 text; one type per file where practical.
- Naming: `PascalCase` for types/methods/properties, `camelCase` for locals/parameters, interfaces prefixed with `I` (for example `IJournalWriter`).
- MVVM naming in UI layer: `*Vm` for view models (`MainVm`, `StartVm`), matching `Views/*Window.xaml`.

## Testing Guidelines
No test project is currently present in this snapshot. Add tests in a sibling project such as `ToadCapture.Tests/` using `Microsoft.NET.Test.Sdk` + xUnit or NUnit.

- Test file naming: `<ClassName>Tests.cs`
- Test method naming: `MethodName_State_ExpectedResult`
- Run tests: `dotnet test` after test projects are added

## Commit & Pull Request Guidelines
Git history is not available in this workspace snapshot, so existing commit conventions could not be inferred directly. Use clear, imperative commit messages and prefer Conventional Commit style (for example `feat(persistence): add journal rebuild guard`).

PRs should include:
- concise summary and rationale
- linked issue/ticket
- test notes (what was run, or why tests are missing)
- UI screenshots/GIFs for WPF view changes
