# ShroudStudio

ShroudStudio is an early-stage Windows/WPF research tool for inspecting
Enshrouded savegames and local game data.

## Current state

The current read-only prototype can:

- inspect KSC1 containers and character payloads;
- identify inventory containers and saved slots;
- correlate slots with verified item instances;
- resolve local item definitions where exported game data is available;
- show quantities, item levels, damage values, rarity codes, descriptions,
  equipment categories, component values, sockets, perks/buff references and
  other observed instance fields;
- keep technical diagnostics separate from the player-facing inventory view.

Savegame writing, round-trip validation, runtime cheats and a complete
official localization mapping are intentionally not enabled yet.

## Requirements

- Windows
- .NET 10 SDK/runtime
- A local Enshrouded installation for game-data discovery

## Build

```powershell
dotnet test .\tests\ShroudStudio.Tests\ShroudStudio.Tests.csproj
dotnet run --project .\src\ShroudStudio.App\ShroudStudio.App.csproj
```

The project is experimental. Always keep independent backups of savegames.
