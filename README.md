# Easy Social Media Plugin

A Dalamud plugin that opens social media profiles advertised in nearby players' FFXIV search
comments. It currently resolves F-list profiles (`/c/` marker) and X handles (`@handle`).

## Rules

### F-list

- `/c/`, `/c`, or `c/` uses the player's FFXIV character name.
- `c/ProfileName` or `/c/ProfileName` uses the explicit profile name when it contains only letters,
  underscores, or hyphens and is at least six characters long. Digits, other punctuation, or a
  shorter name make it fall back to the player's FFXIV character name.
- Without one of those markers, the plugin does not construct or open an F-list profile.

### X

- An `@handle` in the search comment (1-15 letters, digits, or underscores) opens that X profile.

## Installation

Add this URL under Dalamud Settings > Experimental > Custom Plugin Repositories:

```text
https://raw.githubusercontent.com/SaekoFFXIV/DalamudPlugins/main/repo.json
```

Then search for **Easy Social Media Plugin** in the Dalamud Plugin Installer.

## Usage

When an Adventurer Plate or Examine window contains a valid marker, an **F-list ↗** and/or **X ↗**
button appears beside that window and opens the resolved profile immediately.

To open the targeted player's F-list profile, right-click them and choose **Open F-list Profile**,
or target them and run `/social`. The game briefly opens the Examine window while it retrieves the
player's current search comment.

## Build

Requires the .NET 10 SDK and a current Dalamud installation:

```powershell
dotnet build .\EasySocialMediaPlugin.sln -c Release
```

The development DLL is produced at `EasySocialMediaPlugin\bin\x64\Release\EasySocialMediaPlugin.dll`.
