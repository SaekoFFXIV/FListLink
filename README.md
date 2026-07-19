# F-List Link

A Dalamud plugin that opens F-list profiles advertised in nearby players' FFXIV search comments.

## Rules

- `/c/`, `/c`, or `c/` uses the player's FFXIV character name.
- `c/ProfileName` or `/c/ProfileName` uses the explicit profile name when it contains only letters,
  underscores, or hyphens. Digits and other punctuation make it fall back to the player's FFXIV
  character name.
- Without one of those markers, the plugin does not construct or open a profile.

## Installation

Add this URL under Dalamud Settings > Experimental > Custom Plugin Repositories:

```text
https://raw.githubusercontent.com/kaitFFXIV/DalamudPlugins/main/repo.json
```

Then search for **F-List Link** in the Dalamud Plugin Installer.

## Usage

Right-click a nearby player and choose **Open F-list Profile**, or target them and run `/flink`.
The game briefly opens the Examine window while it retrieves the player's current search comment.
When an Adventurer Plate or Examine window contains a valid marker, an **F-list link** button
appears beside that window and opens the resolved profile immediately.

## Build

Requires the .NET 10 SDK and a current Dalamud installation:

```powershell
dotnet build .\FListLink.sln -c Release
```

The development DLL is produced at `FListLink\bin\x64\Release\FListLink.dll`.
