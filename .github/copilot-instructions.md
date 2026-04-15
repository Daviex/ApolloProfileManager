# Copilot Instructions — Apollo Profile Manager

## What This Project Is

A **Windows-only WinForms app** (C# / .NET 10) that hooks into [Apollo](https://github.com/ClassicOldSong/Apollo) (a game-streaming host) to automatically swap per-client game saves, config files and mod sets when a streaming session starts and ends.

The executable is named `manager.exe` (set via `<AssemblyName>` in the `.csproj`).

---

## Repository Layout

All source files live **at the repository root** — there is no `src/` subdirectory.

| File | Purpose |
|---|---|
| `Program.cs` | Entry point. Dispatches CLI commands (`restore`, `save`), hidden flags (`--show-error-dialog`, `--inject-config`), and GUI mode. |
| `ProfileManagerGUI.cs` / `.Designer.cs` / `.resx` | Main WinForms window. Lists Apollo apps, shows last-run/save metadata, opens sub-dialogs. |
| `PathEditorDialog.cs` / `.Designer.cs` / `.resx` | Dialog to add/remove tracked file/folder paths for an app. Supports drag-and-drop. Validates parent/child conflicts. |
| `ClientManagerDialog.cs` / `.Designer.cs` / `.resx` | Dialog to inspect/delete per-client save directories. |
| `ProfileEngine.cs` | Core save/restore logic — `DoAction(appDir, clientDir, paths, "restore"\|"save")`. |
| `ApolloConfigHelper.cs` | Parses `sunshine.conf` (sectionless INI), reads `file_apps` key, injects `global_prep_cmd` entries. |
| `ApolloConfigPathSelector.cs` | Reads/persists Apollo config path in `config.ini`; prompts on first run. |
| `PathHelper.cs` | `GetBaseDir()`, `MakeRel()`, `Sha1Hex()`, `GetAppPaths()` / `SetAppPaths()`. |
| `IniHelper.cs` + `IniConfig` | Custom INI read/write (mimics Python `ConfigParser(interpolation=None)`). Keys stored lowercase, UTF-8. |
| `FileSystemHelper.cs` | `CopyItem()` / `RemoveItem()` — recursive copy/delete, symlink-aware. |
| `ErrorDialogHelper.cs` | Spawns a detached process with `--show-error-dialog <base64>` to show errors after the main process exits. |
| `AdminHelper.cs` | `IsElevated()` — checks for Windows admin rights via `WindowsPrincipal`. |
| `ApolloProfileManager.csproj` | Target: `net10.0-windows`, WinForms, self-contained, `win-x64`, single-file publish. |
| `ApolloProfileManager.sln` | Solution file (single project). |

---

## Key Design Decisions

### Data Directory
User-persistent data lives in **`%LOCALAPPDATA%\ApolloProfileManager\`** (via `PathHelper.GetAppDataDir()`), not next to the executable. This means profiles and config survive rebuilds, reinstalls, or moving the exe.

- `config.ini` → `%LOCALAPPDATA%\ApolloProfileManager\config.ini`
- profiles → `%LOCALAPPDATA%\ApolloProfileManager\profiles\` (default; overridable via `[settings] profiles_dir`)

`PathHelper.GetBaseDir()` still exists but is only used if the exe's own location is needed (e.g. resolving the exe path for `--inject-config`).

### Profile Storage Structure
```
%LOCALAPPDATA%\ApolloProfileManager\
  config.ini               # [apollo] apollo_config_path
                           # [settings] profiles_dir  (optional override; empty = use default)
  profiles/
    <app-uuid>/
      profile.ini          # [meta] app_name, last_run_time/client, last_save_time/client
                           # [paths] sha1hash = /absolute/path
      <client-uuid>/
        client.ini         # [meta] client_name, last_run_time, last_save_time
        <sha1>             # directory copy (no extension)
        <sha1>.ext         # file copy (preserves extension)
      __backup_<client-uuid>/   # one-time backup of original files before first restore
        <sha1>  /  <sha1>.ext
```

### Path Hashing
Paths are keyed by **SHA-1 hex** of the UTF-8 path string (`PathHelper.Sha1Hex`). File copies preserve the original extension; directory copies have no extension.

### Apollo Hook Mechanism
`ApolloConfigHelper.InjectPrepCommands()` prepends a JSON object to `global_prep_cmd` in `sunshine.conf`:
```json
{"do": "\"manager.exe\" restore", "undo": "\"manager.exe\" save", "elevated": true}
```
The `--inject-config <path>` hidden flag lets the app re-inject with UAC elevation via `ShellExecute("runas", ...)`.

### Error Dialog Pattern
When `Program.Main` catches an unhandled exception it calls `ErrorDialogHelper.SpawnErrorDialogAndExitZero()` which:
1. Base64-encodes the error message
2. Starts a detached `manager.exe --show-error-dialog <base64>` process
3. Exits the current process with code 0 (so Apollo doesn't think the hook failed)

### IniConfig
`IniConfig` is a thin wrapper over `Dictionary<string, Dictionary<string, string>>` with case-insensitive section and key lookup. `IniHelper.LoadConfig(path, defaults)` merges defaults without overwriting existing values (mirrors Python `setdefault`).

### `sunshine.conf` Parsing
The file has no section headers. `ApolloConfigHelper.ParseApolloConfig()` prepends `[root]` before parsing so the standard INI reader works; the section is accessed as `cfg["root"]`.

---

## Build & Publish

```bash
# Debug
dotnet build

# Self-contained single-file release exe
dotnet publish -c Release
# Output: bin\Release\net10.0-windows\win-x64\publish\manager.exe
```

No NuGet packages are used — the project has zero external dependencies.

---

## Conventions to Follow

- **Namespace**: everything is in `namespace ApolloProfileManager;` (file-scoped).
- **Implicit usings** are enabled; `System`, `System.IO`, `System.Collections.Generic`, `System.Linq`, `System.Windows.Forms` etc. are available without explicit `using` statements.
- **Nullable** is enabled — use `?` annotations and null-forgiving operators appropriately.
- All `static` helpers live in `static class` files; only WinForms dialogs are instanced.
- Designer files (`*.Designer.cs`) are auto-generated — do **not** hand-edit them; use the WinForms designer.
- `*.resx` files are auto-generated resource files — do **not** hand-edit them.
- INI keys are always **lowercase** (enforced by `IniConfig`).
- Path comparisons use `StringComparer.OrdinalIgnoreCase` (Windows paths are case-insensitive).
- `ProfileEngine` and `PathHelper` are pure static utilities — no WinForms references.
