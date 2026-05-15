# AGENTS.md

## Project Overview

BDO Language Updater is a desktop utility for Black Desert Online players who want to use a game language that is not offered by their server region.

Black Desert Online exposes different language choices depending on the official server region. For example, the South America region offers Spanish and Portuguese, but does not expose English as an in-game language option. This application works around that limitation by downloading the latest official BDO localization file for the target language and applying it to the user's local game files.

The default language target in the current codebase is English, using the official `languagedata_en.loc` localization file source. The broader product goal is to let the user choose the language they want, as long as an official localization file is available for that language.

## Technology Stack

- .NET 9
- Avalonia desktop UI
- The UI project is named `BDOLanguageUpdater.WPF`, but the implementation uses Avalonia rather than classic WPF.
- Microsoft.Extensions.Hosting for application hosting, dependency injection, hosted services, and configuration.
- Microsoft.Extensions.Http for HTTP client registration.
- Newtonsoft.Json for JSON handling.
- Serilog.AspNetCore for logging integration.
- xUnit, Microsoft.NET.Test.Sdk, and coverlet for tests and coverage collection.
- Windows COM interop in `BDOLanguageUpdater.ShortcutGenerator` for shortcut/startup helper behavior.

## Solution Layout

- `BDOLanguageUpdater.Service`: Core updater logic, file management, localization serialization, user preferences, HTTP downloads, and background update/watch services.
- `BDOLanguageUpdater.WPF`: Avalonia desktop application, views, view models, notifications, startup behavior, and UI composition.
- `BDOLanguageUpdater.ShortcutGenerator`: Windows shortcut generation helper.
- `BDOLanguageUpdater.Tests`: xUnit tests for service-level behavior.
- `Directory.Build.props`: Shared .NET project settings, metadata, target framework, nullable settings, and versioning.
- `Directory.Packages.props`: Central NuGet package version management.

## Core Behavior

The application stores the user's BDO installation path, finds the game's localization files directory, downloads the latest official localization file for the configured target language, merges or applies the downloaded localization data, and writes the result into the local BDO client language file path.

This is intended for official BDO servers and official localization assets. The application should not depend on unofficial translation sources unless that is introduced as an explicit feature with separate naming and safeguards.

## Contributor Notes

- Keep localization download, merge, file-system, and update-watcher behavior in `BDOLanguageUpdater.Service`.
- Keep UI concerns in the Avalonia project and avoid mixing presentation logic into the service layer.
- Treat the `BDOLanguageUpdater.WPF` name as historical/project naming; prefer Avalonia APIs and patterns for UI work.
- Use central package management in `Directory.Packages.props` when adding or changing NuGet package versions.
- Preserve the user's configured BDO path and language-related preferences when changing updater behavior.
- Run `dotnet test` after service or serialization changes.
