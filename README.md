# Common Utilities & Frameworks

A collection of **reusable libraries, tools, and utilities** for WPF, data handling, math, rendering, debugging, and more.  
Organized into modular projects that can be used independently or combined.

---

## General Utilities

- **Weaver / Lector** – Lightweight C# command engines for scripting and runtime execution.
  - **Weaver** is the new interpreter, replacing the legacy Interpreter engine. Which is it's own sub Project see: https://github.com/LoneWandererProductions/Lector
  - **Lector** is the CommandLine integration of Weaver, providing CLI command execution and script parsing.
  - Supports namespaces, extensions, interactive feedback, command registration, structured scripts, loops, conditionals, variables, and debugging.

- **Custom Controls** – Reusable WPF UI components (forms, trees, tables, dialogs).
- **Extension Methods** – Helpers for `List<T>`, `Dictionary<K,V>`, and other types.
- **SQLite Extensions & GUI** – Methods and WPF frontend for SQLite browsing and editing.
- **Generic Serializer** – XML-based serialization for complex objects.
- **Fast Lookup Dictionary** – Optimized for high-speed dictionary access.
- **RAM Memory Cache** – Temporary in-memory object storage.
- **Memory Manager** – Experimental custom memory management.
- **Image Manipulation & Comparison** – Filters, transformations, and pixel comparison tools.
- **File Handling Abstraction** – Unified interface for copy, move, delete, metadata, etc.
- **CSV Reader/Writer** – Simple serialization and I/O utility.
- **Vector Graphics Implementation** – Basic rendering of vector shapes and paths.
- **3D Projection Tools** – Experimental 3D projection and transformation.
- **Debugging & Logging Framework** – Structured logging with optional UI integration.
- **Win32k Wrappers** – Access low-level system features like registry and native dialogs.
- **Plugin Interface** – Loading/unloading and discovery support for plugins.
- **Pathfinder** – Grid-based A* pathfinding for turn-based maps.
- **Dialogs** – Prebuilt SQL login, folder pickers, and confirmation dialogs.

---

## Math Utilities

- **Matrix Calculations** – Addition, multiplication, inversion, etc.
- **3D Vector Calculations** – Vector math for rendering, physics, and pathfinding.
- **Fractals** – Procedural fractal generation tools.
- **Statistics** – Mean, median, variance, and basic analysis.

---

## Coding & Framework Tools

- **Resource String Generator** – Strongly typed classes from raw resource files.
- **In-Memory Logger (`CoreMemoryLog`)** – Runtime log capture without disk I/O.
- **Simple Dependency Injection** – Lightweight DI container.
- **Worker Service Framework** – Abstraction for background threads and long-running jobs.

---

## Highlighted Modules

### Debugger

Flexible **log inspection and debugging framework**.

- Defines `ILogSource` abstraction for plug-and-play log providers.
- Supports event-driven log streaming (`LineReceived`) for real-time UI updates.
- Includes adapters like `InMemoryLogSource` to visualize `CoreMemoryLog`.

**Example:**
```csharp
var source = new InMemoryLogSource(logger);
source.LineReceived += (_, line) => Console.WriteLine(line);
source.Start();
```

---

### CoreMemoryLog

**Lightweight, structured, in-memory logger** for diagnostics.

- Rolling buffer of log entries (timestamp, level, caller, optional exception).
- Can integrate with `Debugger` UI or be used directly in tests.

**Example:**
```csharp
var logger = new InMemoryLogger();
logger.Log(LogLevel.Info, "Starting job {0}", jobId);

foreach (var entry in logger.GetLogs())
    Console.WriteLine($"{entry.Timestamp}: {entry.Message}");
```

---

### CoreBuilder & Code Analyzer

**Scriptable code analyzers and developer utilities**, fully integrated with **Weaver**.

- Detect unused code, formatting issues, missing comments, allocations, etc.
- Utilities for header extraction, license blocks, and string resources.
- Lightweight API exploration (reflection metadata).
- Filesystem utilities (directory size, complexity checks).

**Integration:** Can be executed from:
- GUI via **CoreViewer**
- CLI using **Weaver command engine**
- Scripts inside **Weaver Script Engine**

**Supporting Projects:**
- **CoreViewer** – GUI for visualizing analysis results.
- **CommonDialogs** – File/folder pickers and confirmation prompts.
- **ViewModel** – MVVM bindings for GUI interactions.

---

## Project Overview

| Module                      | Purpose / Description                           |
| --------------------------- | ----------------------------------------------- |
| `Weaver`                    | New command interpreter replacing legacy Lector |
| `Lector`                    | CLI integration of Weaver                       |
| `CommonLibraryTests`        | Unit tests for common libraries                 |
| `CommonExtendedObjectsTests`| Unit tests for ExtendedSystemObjects            |
| `Imaging`                   | Image manipulation tools                        |
| `CommonControls`            | Shared WPF custom controls                      |
| `SQLiteHelper`              | SQLite DB interaction & abstraction             |
| `Mathematics`               | Math utilities and calculation libraries        |
| `CommonLibrarySqlLiteTests` | Tests for SQLite-related features               |
| `ExtendedSystemObjects`     | Extensions for .NET base classes                |
| `FileHandler`               | File system abstraction                          |
| `RenderEngine`              | Experimental 2D/3D rendering backend            |
| `RenderEngineTests`         | Rendering-related tests                          |
| `SQLiteGui`                 | UI frontend for SQLiteHelper                    |
| `Debugger`                  | Debugging framework + log viewer integration    |
| `ImageCompare`              | Pixel-based image comparison utility            |
| `CommonDialogs`             | Prebuilt WPF dialogs                             |
| `CommonLibraryGuiTests`     | GUI testing for common components               |
| `Solaris`                   | 2D chessboard-style tile renderer               |
| `LightVector`               | Lightweight vector math tools                   |
| `Serializer`                | XML serialization of data and structures        |
| `DataFormatter`             | CSV reading/writing and formatting              |
| `Communication`             | Networking and web service helpers              |
| `CoreBuilderTests`          | Tests for image/vector building tools           |
| `PluginLoader`              | Plugin management and loading system            |
| `CommonFilter`              | WPF control to filter data tables               |
| `CoreBuilder`               | Utilities for image/vector creation             |
| `Pathfinder`                | A* pathfinding for grid-based maps              |
| `InterOp`                   | Windows API bindings                             |
| `CoreInject`                | Minimal DI/injection support                    |
| `CoreConsole`               | Quick console runner for apps                   |
| `CoreMemoryLog`             | In-memory logger, structured, UI-ready          |
| `Plugin`                    | Base classes/interfaces for plugin system       |
| `ViewModel`                 | ViewModel bindings for WPF MVVM support         |
| `CoreWorker`                | Worker service abstraction                      |

