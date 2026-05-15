# BDO Language Updater

BDO Language Updater is a desktop app that replaces one installed Black Desert Online regional language file with the latest official English localization file.

This is useful when your server region does not expose English in the game settings. For example, South America normally offers Spanish and Portuguese, but not English. The app detects the installed `languagedata_*.loc` files and lets you choose which one should be replaced with English.

## Download

Download the latest build from [GitHub Releases](https://github.com/PadoruCorp/bdo-language-updater/releases).

- Windows: download `BDOLanguageUpdater-win-x64.zip`, extract it, and run `BDOLanguageUpdater.exe`.
- Linux: download `BDOLanguageUpdater-linux-x64.tar.gz` if you run BDO through a compatible Linux setup.

Release builds are self-contained, so users should not need to install .NET separately.

## How To Use

1. Open `BDOLanguageUpdater`.
2. Select your Black Desert Online folder.
3. Click `Scan`.
4. Choose the installed language file you want to replace, such as `Spanish (es)` or `Portuguese (pt)`.
5. Click `Update Language`.

The app downloads the latest official English localization and writes it into the selected local language file.

## Launch Options

After selecting the BDO folder and language, you can also use:

- `Update And Launch Steam`: updates the language file, then opens BDO through Steam.
- `Update And Launch Launcher`: updates the language file, then runs `BlackDesertLauncher.exe` from the selected BDO folder.

## Backups

Before replacing a language file, the app creates a backup next to it:

```text
languagedata_es.loc.bdo-language-updater.bak
```

If a backup exists for the selected language, `Restore Backup` becomes available under the language selector. Use it to restore the original regional language file.

## Automatic Updates

BDO patches can overwrite localization files. In the `Advanced` tab, enable `Auto update after game updates` to create a Windows scheduled task.

The scheduled task runs on the selected maintenance day and retries during the day. It only updates the file when needed.

## Command Line

```powershell
BDOLanguageUpdater.exe --update
BDOLanguageUpdater.exe --update --language=es
BDOLanguageUpdater.exe --update-and-launch --launch=steam
BDOLanguageUpdater.exe --update-and-launch --launch=launcher
BDOLanguageUpdater.exe --restore-backup --language=es
```

## Development

Requirements:

- .NET 10 SDK

Useful commands:

```powershell
dotnet restore BDOLanguageUpdater.sln
dotnet build BDOLanguageUpdater.sln --configuration Release
dotnet test BDOLanguageUpdater.Tests/BDOLanguageUpdater.Tests.csproj --configuration Release
```

Main projects:

- `BDOLanguageUpdater.Service`: localization download, merge, serialization, backup, restore, and update logic.
- `BDOLanguageUpdater.WPF`: Avalonia desktop UI.
- `BDOLanguageUpdater.Tests`: regression and performance tests.

## License

MIT. See [LICENSE](LICENSE).
