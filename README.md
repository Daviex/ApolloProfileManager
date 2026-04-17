# Apollo Profile Manager

Apollo Profile Manager is a tool designed to manage and automatically swap game configuration files, save files, mod sets and other user data between different clients of [Apollo](https://github.com/ClassicOldSong/Apollo). It provides a graphical interface for managing profiles, tracking file paths, and handling client-specific saves, making it easy to maintain separate configurations for different users or devices.

Requires [Apollo v0.3.5-alpha.2 or above](https://github.com/ClassicOldSong/Apollo/releases), does not work with Sunshines.

> [!Note]
> Containing AI generated code, manually reviewed, modified and tested.

> [!Warning]
> Use carefully with your game saves. This app takes no responsiblity for any of your data loss.

## Download

You can find the pre-built binary in [Releases](https://github.com/ClassicOldSong/ApolloProfileManager/releases)

## Usage

1. Run `manager.exe`.
2. On first launch, select your Apollo configuration file (typically `sunshine.conf`). The app will check if its prep commands are already registered and offer to inject them.
3. After the main window appears, select a game from the list and use the buttons on the right:
   - **Edit Tracked Files** — add/remove files and folders the manager will swap per client.
   - **Manage Client Saves** — inspect or delete saves for individual clients.
   - **Open Profile Dir** — open the profile folder in Explorer.
   - **Inject Global Prep Commands** — register (or verify) the save/restore hooks in Apollo. If the commands are already present a warning is shown and no changes are made.
   - **Change Apollo Config File** — point the app at a different `sunshine.conf`.
   - **Change Profiles Directory** — override where profiles are stored (default: `%LOCALAPPDATA%\ApolloProfileManager\profiles`). The setting is saved in `config.ini` and takes effect immediately.

## Data Storage

All user data is stored in `%LOCALAPPDATA%\ApolloProfileManager\` so it persists across updates, reinstalls, or moving the executable:

| Path | Contents |
|---|---|
| `config.ini` | Apollo config path and optional settings overrides |
| `profiles\<app-uuid>\` | Per-game profile data and tracked-path list |
| `profiles\<app-uuid>\<client-uuid>\` | Per-client saved files/folders |
| `logs\manager-YYYY-MM-DD.log` | Daily log file (all console output with timestamps) |

## Prerequisites

- Windows 10 or later (x64)
- [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0) — or use the self-contained build from Releases (no runtime needed)

## Building from Source

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022+ **or** the `dotnet` CLI

### Build with the CLI

```bash
git clone https://github.com/ClassicOldSong/ApolloProfileManager
cd ApolloProfileManager

# Debug build (multiple files, for development)
dotnet build

# Self-contained single-file release executable
dotnet publish -c Release
```

The self-contained single-file executable will be placed in `bin\Release\net10.0-windows\win-x64\publish\manager.exe` — no separate runtime or additional files required.

### Build with Visual Studio

Open `ApolloProfileManager.sln`, set the configuration to **Release**, and press **Build → Publish**.

## How It Works

Apollo Profile Manager hooks into Apollo's `global_prep_cmd` mechanism:

- **`manager.exe restore`** — called by Apollo before a session starts; swaps in the client's saved profile.
- **`manager.exe save`** — called by Apollo after a session ends; saves the current state back to the client's profile.

The prep commands are injected **without** `"elevated": true` so that Apollo spawns them via `CreateProcess` and correctly passes the `APOLLO_*` environment variables the app relies on. Elevation is not required because all data lives in `%LOCALAPPDATA%`.

Profiles are stored in `%LOCALAPPDATA%\ApolloProfileManager\profiles\` by default, organised by app UUID and client UUID. The location can be changed from within the app via **Change Profiles Directory**.

## License

MIT