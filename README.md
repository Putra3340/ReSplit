# ReSplit

**ReSplit** is a lightweight, cross-platform speedrun timer built with [Avalonia UI](https://avaloniaui.net/) and .NET 10. It provides real-time split tracking, delta comparisons against personal bests, global hotkey controls, and an extensible plugin system — all inside a compact, always-on-top overlay window.

> This project is a **from-scratch implementation** inspired by LiveSplit's core features, designed to be modular and easily extensible. It is not a wrapper around LiveSplit or any existing timer software.
> Compatible with **LiveSplit `.lss`** split files.

---

## Table of Contents

- [Features](#features)
- [Requirements](#requirements)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Global Hotkeys](#global-hotkeys)
- [Project Structure](#project-structure)
- [File Reference](#file-reference)
  - [Core Application](#core-application)
  - [Models](#models)
  - [Utils](#utils)
  - [Plugins](#plugins)
  - [Sample](#sample)
- [Plugin Development](#plugin-development)
- [License](#license)

---

## Features

- **LiveSplit Compatibility** — Load `.lss` (LiveSplit Splits) files directly.
- **Real-Time Timer** — High-resolution stopwatch updating at ~60 FPS.
- **Split Tracking** — Automatic delta calculation against personal bests with color-coded indicators.
- **Run Prediction** — Displays predicted final time based on current pace.
- **In-Game Time (IGT)** — Supports IGT display via plugins.
- **Global Hotkeys** — Control the timer from any application using numpad keys (via [SharpHook](https://github.com/TolikPyl662/SharpHook)).
- **Plugin System** — Dynamically load/unload `.dll` plugins at runtime with shadow-copy isolation.
- **Always-on-Top Overlay** — Acrylic blur, borderless, draggable window that stays above all other windows.
- **Segment Creator** — Built-in dialog to create and save new segments with screenshots.

---

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10 or later (for full acrylic blur and WebView support)

### NuGet Dependencies

| Package | Version | Purpose |
|---|---|---|
| `Avalonia` | 11.3.11 | UI framework |
| `Avalonia.Desktop` | 11.3.11 | Desktop platform support |
| `Avalonia.Themes.Fluent` | 11.3.11 | Fluent design theme |
| `Avalonia.Fonts.Inter` | 11.3.11 | Inter font family |
| `Avalonia.Diagnostics` | 11.3.11 | Debug-only diagnostics overlay |
| `SharpHook` | 7.1.1 | Global keyboard hooks |

---

## Getting Started

```bash
# Clone the repository
git clone https://github.com/Putra3340/ReSplit.git
cd ReSplit

# Restore dependencies and build
dotnet restore
dotnet build

# Run the application
dotnet run
```

---

## Usage

1. **Right-click** on the window to open the context menu.
2. Select **Load** to open a `.lss` file.
3. Select **Split** (or press `Numpad 0`) to start the timer and advance to the next split.
4. The timer displays **delta times** (time gained/lost) compared to your personal best.
5. Use the context menu or hotkeys to **Pause**, **Reset**, **Skip**, or **Undo** splits.
6. Select **Load DLL** to load a plugin at runtime.

### Context Menu Options

| Option | Description |
|---|---|
| **Load** | Open a `.lss` file to load segments |
| **Load DLL** | Load a ReSplit plugin (`.dll`) |
| **Split** | Start the timer or advance to the next split |
| **Create Segment** | Open the segment creation dialog |
| **Skip** | Skip the current split |
| **Pause** | Pause the timer |
| **Undo** | Undo the last split |
| **Reset** | Reset the entire run |
| **Exit** | Close the application |

---

## Global Hotkeys

Hotkeys work globally — the application window does **not** need to be focused.

| Key | Action |
|---|---|
| `Numpad 0` | Start / Split |
| `Numpad 4` | Undo Split |
| `Numpad 6` | Skip Split |
| `Numpad 7` | Pause |
| `Numpad 9` | Reset Run |

---

## Project Structure

```
ReSplit/
├── App.axaml / App.axaml.cs          # Application entry & theme
├── Program.cs                         # Main entry point & Avalonia builder
├── MainWindow.axaml / .axaml.cs       # Primary timer window (UI + logic)
├── CreateSegmentWindow.axaml / .cs    # Segment creation dialog
├── GlobalTimer.cs                     # High-resolution stopwatch (~60 FPS)
├── ReSplit.csproj                     # Project configuration & dependencies
├── app.manifest                       # Windows application manifest
│
├── Models/
│   ├── RunModel.cs                    # XML-serializable run data model
│   ├── StaticBinding.cs               # Global state (splits + current run)
│   └── Form/
│       ├── SplitsModel.cs             # Observable split row model
│       └── EnumModel.cs               # TimerState enum
│
├── Utils/
│   ├── CentralControls.cs            # Core timer control logic
│   ├── GlobalHotkeyService.cs         # Global keyboard hook service
│   ├── RunSerializer.cs               # XML serializer for .lss files
│   └── TimeSpanFormat.cs              # Time formatting utilities
│
├── Plugins/
│   ├── PluginLoader.cs                # Dynamic plugin load/unload system
│   └── ResplitPlugins/
│       └── LiveGuide/                 # Example plugin project
│
├── Sample/
│   └── God of War - Platinum EMU.lss  # Sample LiveSplit splits file
│
└── Properties/
    └── PublishProfiles/               # .NET publish profiles
```

---

## File Reference

### Core Application

#### `Program.cs`
The application entry point. Configures the Avalonia `AppBuilder` with:
- Platform detection (`UsePlatformDetect`)
- Inter font (`WithInterFont`)
- WebView initialization with dev tools enabled and a custom `webview-data` folder
- Trace-level logging

#### `App.axaml` / `App.axaml.cs`
Avalonia application definition. Loads XAML resources and applies the **Fluent** theme. On startup, creates and shows the `MainWindow`.

#### `MainWindow.axaml` / `MainWindow.axaml.cs`
The primary timer window. Key characteristics:
- **Fixed size**: 325 × 700 pixels
- **Always-on-top** with `TransparencyLevelHint="AcrylicBlur"` and no system decorations
- **Draggable** via left-click on the window body
- **Context menu** for all timer operations (Load, Split, Reset, etc.)

**UI Sections:**
- Game title, category, and platform labels
- Scrollable splits list bound to `StaticBinding.Splits`
- Main timer display (hours:minutes:seconds + milliseconds)
- Run prediction time
- In-Game Time (IGT) display
- Debug/status info line

**Code-Behind Logic:**
- `OpenRunFileAsync()` — Opens a file picker filtered for `.lss` files
- `SetupLoad()` — Deserializes the `.lss` file, populates splits, and sets the first split as active
- `MenuItem_Click()` — Routes context menu actions to `CentralControls` or opens dialog windows

#### `CreateSegmentWindow.axaml` / `CreateSegmentWindow.axaml.cs`
A modal dialog for creating new segments. Features:
- Text input for segment name
- File browser for selecting a screenshot image (`.png`, `.jpg`, `.jpeg`, `.webp`)
- Saves segments as a `segments.json` file in the application directory
- Each segment record contains: `Name`, `Screenshot` (path), and `State` (current debug info)

#### `GlobalTimer.cs`
A static high-resolution timer using `System.Diagnostics.Stopwatch` and `System.Timers.Timer`.
- Ticks every **16ms** (~60 FPS) and pushes UI updates via `Dispatcher.UIThread`
- Formats time display adaptively (hours:min:sec or min:sec depending on elapsed time)
- Provides `Start()`, `Stop()`, `Reset()`, and `GetElapsedTime()` methods

---

### Models

#### `Models/RunModel.cs`
XML-serializable data model matching the **LiveSplit `.lss`** file format:

| Class | Description |
|---|---|
| `RunModel` | Root element — contains `GameName`, `CategoryName`, `Platform`, `Metadata`, and `Segments` |
| `Metadata` | Run metadata including `AttemptCount` |
| `Segment` | Individual segment with `Id`, `Name`, and a list of `SplitTimes` |
| `SplitTime` | A comparison entry (e.g. "Personal Best") with `RealTime` as a string |

#### `Models/StaticBinding.cs`
Global static state holder for the application:
- `Splits` — `ObservableCollection<SplitsModel>` bound to the UI list. Pre-populated with example data for design-time preview.
- `CurrentRun` — The currently loaded `RunModel` instance.

#### `Models/Form/SplitsModel.cs`
Observable model for a single split row in the UI. Implements `INotifyPropertyChanged` for live data binding.

| Property | Type | Description |
|---|---|---|
| `Id` | `string` | Unique identifier |
| `Name` | `string` | Display name of the segment |
| `Time` | `TimeSpan` | Personal best time for this split |
| `NewTime` | `TimeSpan` | Current attempt's time for this split Special values: , `MinValue` = no time (-)|
| `DeltaTime` | `TimeSpan` | Time difference vs. PB. Special values: `MaxValue` = no delta (blank value), `MinValue` = new time (-) |
| `IsActive` | `bool` | Whether this is the currently active split (highlighted blue) |
| `BackgroundColor` | `string` | Row background color (`"Blue"` when active, `"Transparent"` otherwise) |
| `DeltaForegroundColor` | `string` | Color for delta display (default: `#FFFFC000` amber) |

**Display Properties** (prefixed with `F_`):
- `F_Name`, `F_Time`, `F_DeltaTime`, `F_BackgroundColor` — Formatted read-only properties for XAML binding.

#### `Models/Form/EnumModel.cs`
Defines the `TimerState` enum:

```csharp
public enum TimerState
{
    NotStarted,  // Timer has not been started
    Running,     // Timer is actively running
    Paused,      // Timer is paused
    LosingTime,  // Current pace is behind PB
    GainingTime, // Current pace is ahead of PB
    Ended        // Run has been completed
}
```

---

### Utils

#### `Utils/CentralControls.cs`
Central control logic for all timer operations. All methods are **static** and operate on `StaticBinding.Splits`.

| Method | Description |
|---|---|
| `StartNewAttempt()` | Starts the timer if not running; calls `Split()` if already running |
| `Split()` | Records the current time for the active split, calculates delta, and advances to the next split |
| `UndoSplit()` | Reverts to the previous split, restoring its original time |
| `SkipSplit()` | Skips the current split (marks delta as `MaxValue`) and moves to the next |
| `ResetRun()` | Stops the timer, resets all splits to their original times, and reactivates the first split |
| `Pause()` | Pauses the global timer |
| `UpdateTimerState()` | Updates the timer state and changes the timer text color accordingly |

**Timer Color Scheme:**

| State | Color |
|---|---|
| `NotStarted` | White (`#FFFFFFFF`) |
| `Running` | Green (`#FF00CC36`) |
| `Paused` | Gray (`#FF7A7A7A`) |
| `LosingTime` | Red (`#FFFF0000`) |
| `GainingTime` | Bright Green (`#FF00FF00`) |
| `Ended` | Blue (`#FF3B82F6`) |

#### `Utils/GlobalHotkeyService.cs`
System-wide keyboard hook using [SharpHook](https://github.com/TolikPyloy/SharpHook). Runs on a background thread and listens for numpad key presses to trigger timer actions.

#### `Utils/RunSerializer.cs`
XML serialization/deserialization utility for `.lss` files:
- `Load(string path)` — Deserializes a `.lss` file into a `RunModel`
- `Save(RunModel run, string path)` — Serializes a `RunModel` to XML

#### `Utils/TimeSpanFormat.cs`
Formatting utilities for time display:

| Method | Description |
|---|---|
| `FormatDelta(TimeSpan)` | Formats a delta with `+`/`-` prefix. Returns `"-"` for `MinValue` (new time), `""` for `MaxValue` (skipped) |
| `FormatNewTime(TimeSpan)` | Formats an absolute time adaptively: `H:MM:SS`, `MM:SS`, or `"00:00"` |

---

### Plugins

#### `Plugins/PluginLoader.cs`
Dynamic plugin loading system with full lifecycle management.

**Key Interfaces & Classes:**

##### `IReSplitHost` (Interface)
The host interface that plugins receive for communicating with ReSplit:

| Member | Description |
|---|---|
| `Splits` | Access to the observable splits collection |
| `IdentifierPath` | The DLL file path used to identify the plugin |
| `SetStatus(string)` | Update the status/debug text on the main window |
| `UpdateIGT(TimeSpan)` | Update the In-Game Time display |
| `StartOrSplit()` | Trigger a start or split action |
| `Reset()` | Reset the current run |
| `Shutdown(string)` | Request the host to unload this plugin |

##### `PluginLoader` (Static Class)
Manages loading, unloading, and reloading plugins:

| Method | Description |
|---|---|
| `LoadAndInitialize(Window)` | Opens a file picker and loads selected `.dll` files |
| `LoadPlugin(string)` | Loads a single plugin by path with shadow-copy isolation |
| `UnloadPlugin(string)` | Unloads a plugin and cleans up temporary files |
| `ReloadPlugin(string)` | Unloads then reloads a plugin (hot-reload) |
| `UnloadAllPlugins()` | Unloads all currently loaded plugins |
| `IsLoaded(string)` | Checks if a plugin is currently loaded |

**Plugin Loading Process:**
1. The DLL is **shadow-copied** to a temp directory to avoid file locking
2. A collectible `AssemblyLoadContext` loads the assembly
3. The loader scans for a class containing:
   - `public static string Name { get; }` 
   - `public static string Description { get; }`
   - `public static void Initialize(IReSplitHost host)`
4. `Initialize()` is called with an `IReSplitHost` instance
5. On unload, the `AssemblyLoadContext` is unloaded and temp files are deleted

#### `Plugins/ResplitPlugins/LiveGuide/` (Example Plugin)
A reference plugin implementation demonstrating the plugin API:
- **`LiveGuide.cs`** — Main plugin class with required `Name`, `Description`, and `Initialize()`. Opens a separate Avalonia window on initialization.
- **`LiveGuide.csproj`** — Plugin project referencing the main `ReSplit.csproj`. Includes a post-build step to copy the DLL to the main app's output directory.

---

### Sample

#### `Sample/God of War - Platinum EMU.lss`
A sample LiveSplit splits file for "God of War — Platinum" category. Contains:
- 2 segments: "Ares" and "Challenge 10"
- 6 attempt entries with real-time data
- Personal best split times

This file can be used to test the application's `.lss` loading functionality.

---

## Plugin Development

To create a ReSplit plugin:

1. **Create a new Class Library** project targeting `.NET 10`.
2. **Reference** the main `ReSplit.csproj` or import the `IReSplitHost` interface.
3. **Create a plugin class** with the following required members:

```csharp
public class MyPlugin
{
    // Required properties
    public static string Name => "My Plugin";
    public static string Description => "Description of my plugin";

    // Required entry point
    public static void Initialize(IReSplitHost host)
    {
        // host.Splits        — access the splits collection
        // host.SetStatus()   — update status text
        // host.UpdateIGT()   — update in-game time
        // host.StartOrSplit() — trigger split
        // host.Reset()       — reset the run
        // host.Shutdown()    — request unload
    }
}
```

4. **Build** the project and load the resulting `.dll` via **Load DLL** in the context menu.

> See `Plugins/ResplitPlugins/LiveGuide/` for a complete working example.

---

## License

This project is licensed under the terms specified in [LICENSE.txt](LICENSE.txt).

---

*Made by **Putra3340***