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
2. When prompted, select your Apollo configuration file (typically `sunshine.conf`).
3. After the main application window appears, you can add files that you want the manager to track via the **Edit Paths** button.
4. Click **Inject Prep Commands** to automatically register the save/restore hooks in your Apollo config.

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
dotnet publish -c Release
```

The self-contained executable will be placed in `bin\Release\net10.0-windows\win-x64\publish\manager.exe`.

### Build with Visual Studio

Open `ApolloProfileManager.sln`, set the configuration to **Release**, and press **Build → Publish**.

## How It Works

Apollo Profile Manager hooks into Apollo's `global_prep_cmd` mechanism:

- **`manager.exe restore`** — called by Apollo before a session starts; swaps in the client's saved profile.
- **`manager.exe save`** — called by Apollo after a session ends; saves the current state back to the client's profile.

Profiles are stored in a `profiles\` directory next to `manager.exe`, organised by app UUID and client UUID.

## License

MIT