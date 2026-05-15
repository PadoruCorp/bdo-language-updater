# AGENTS.md

## Project Overview

BDO Language Updater is a desktop utility for Black Desert Online players who want to use a game language that is not offered by their server region.

Black Desert Online exposes different language choices depending on the official server region. For example, the South America region offers Spanish and Portuguese, but does not expose English as an in-game language option. This application works around that limitation by downloading the latest official BDO localization file for the target language and applying it to the user's local game files.

The default language target in the current codebase is English, using the official `languagedata_en.loc` localization file source. The broader product goal is to let the user choose the language they want, as long as an official localization file is available for that language.

## Technology Stack

- .NET 10
- Avalonia desktop UI
- The UI project is named `BDOLanguageUpdater.WPF`, but the implementation uses Avalonia rather than classic WPF.
- Microsoft.Extensions.Hosting for application hosting, dependency injection, and configuration.
- Microsoft.Extensions.Http for HTTP client registration.
- System.Text.Json for JSON handling.
- Serilog.Extensions.Hosting with file/debug sinks for logging.
- xUnit, Microsoft.NET.Test.Sdk, and coverlet for tests and coverage collection.
- GitHub Actions release workflow publishes self-contained Windows and Linux binaries.

## Solution Layout

- `BDOLanguageUpdater.Service`: Core updater logic, file management, localization serialization, user preferences, HTTP downloads, backups, restore behavior, and update metadata.
- `BDOLanguageUpdater.WPF`: Avalonia desktop application, views, view models, tray behavior, scheduled-task setup, Steam/launcher launch flows, and UI composition.
- `BDOLanguageUpdater.Tests`: xUnit tests for service behavior, localization structure, metadata, backups, and performance-sensitive paths.
- `Directory.Build.props`: Shared .NET project settings, metadata, target framework, nullable settings, and versioning.
- `Directory.Packages.props`: Central NuGet package version management.
- `.github/workflows/release.yml`: Release workflow for self-contained `win-x64` and `linux-x64` builds.

## Core Behavior

The application stores the user's BDO installation path, finds the game's `ads` localization directory, detects installed `languagedata_*.loc` files, downloads the latest official English localization file, merges English text into the selected regional language file, and writes the result back to the local BDO client.

Before replacing a selected language file, the updater creates a side-by-side `.bdo-language-updater.bak` backup unless the current file is already known to be produced by the updater. The metadata sidecar is used to skip unnecessary rewrites and to avoid replacing the original-language backup with an already-patched English file.

The desktop UI supports updating only, updating and launching through Steam, updating and launching through `BlackDesertLauncher.exe`, restoring backups, and configuring a Windows scheduled task for automatic weekly updates after game maintenance.

This is intended for official BDO servers and official localization assets. The application should not depend on unofficial translation sources unless that is introduced as an explicit feature with separate naming and safeguards.

## Contributor Notes

- Keep localization download, merge, file-system, backup, restore, and metadata behavior in `BDOLanguageUpdater.Service`.
- Keep UI concerns in the Avalonia project and avoid mixing presentation logic into the service layer.
- Treat the `BDOLanguageUpdater.WPF` name as historical/project naming; prefer Avalonia APIs and patterns for UI work.
- Use central package management in `Directory.Packages.props` when adding or changing NuGet package versions.
- Preserve the user's configured BDO path, selected replacement language, backup files, and scheduled update preferences when changing updater behavior.
- Do not reintroduce the removed shortcut-generator or notification dependencies unless the feature is explicitly requested again.
- Run `dotnet test` after service or serialization changes.
