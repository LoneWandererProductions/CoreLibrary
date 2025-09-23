# Common Utilities & Frameworks

This repository provides a collection of reusable libraries, tools, and utilities for WPF, data handling, math, rendering, debugging, and more.  
It’s organized into modular projects that can be used independently or combined.

---

## General Utilities

- **Generic Interpreter**  
  Command-line-like interpreter for executing user-defined commands or internal scripting.

- **Custom Controls**  
  Reusable WPF UI components for forms, trees, tables, and dialogs.

- **Extension Methods**  
  Helpers for extending native types like `List<T>`, `Dictionary<K,V>`, and more.

- **SQLite Extensions**  
  Methods and UI helpers for SQLite interactions; includes GUI frontend for data browsing and manipulation.

- **Generic Serializer**  
  XML-based serialization for lists, dictionaries, and complex objects.

- **Fast Lookup Dictionary**  
  Optimized data structure for extremely fast dictionary lookups.

- **RAM Memory Cache**  
  In-memory cache store for temporary object persistence.

- **Memory Manager**  
  Experimental work on custom memory management.

- **Image Manipulation & Comparison**  
  Libraries for editing and comparing images; includes filters and format utilities.

- **File Handling Abstraction**  
  Unified file operation interface: copy, move, delete, metadata, etc.

- **CSV Reader/Writer**  
  Simple I/O utility to read/write CSV files and serialize object data.

- **Vector Graphics Implementation**  
  Basic tools for rendering vector shapes and paths.

- **3D Projection Tools**  
  Simple 3D projection and transformation library for experimental visual rendering.

- **Debugging & Logging Framework**  
  Structured logging and in-memory debug tracing, optionally UI-bound.

- **Win32k Wrappers**  
  Access low-level system features like registry editing and native dialogs.

- **Plugin Interface**  
  Plugin system with loading/unloading and discovery support.

- **Pathfinder**  
  Grid-based A* algorithm for turn-based strategy/pathfinding needs.

- **Dialogs**  
  Custom dialogs including SQL login, folder pickers, etc.

---

## Math Utilities

- **Matrix Calculation**  
  Basic matrix math: addition, multiplication, inversion, etc.

- **3D Vector Calculations**  
  Vector math used in rendering, physics, and pathfinding.

- **Fractals**  
  Tools for procedural generation of fractal imagery.

- **Statistics**  
  Calculate mean, median, variance, and basic statistical analysis.

---

## Coding & Framework Tools

- **Resource String Generator**  
  Auto-generates strongly typed string classes from raw resource files.

- **In-Memory Logger (CoreMemoryLog)**  
  Lightweight logger that keeps runtime logs in memory.  
  See [CoreMemoryLog](#corememorylog) for details.

- **Simple Dependency Injection**  
  Minimal DI container to inject and manage services across projects.

- **Worker Service Framework**  
  Manages background threads and long-running jobs with clean abstraction.

---

## Highlighted Modules

### Debugger

A flexible **debugging and log inspection framework**.

- Defines the `ILogSource` abstraction so log providers (memory, file, stream, external) can be plugged in and consumed uniformly.  
- Supports event-driven log streaming (`LineReceived`) for real-time UI updates.  
- Includes adapters like `InMemoryLogSource` to expose `CoreMemoryLog` into UI log viewers.  
- Designed for WPF dashboards and log viewer tools.

**Skeleton usage:**
```csharp
var source = new InMemoryLogSource(logger);
source.LineReceived += (_, line) => Console.WriteLine(line);
source.Start();
```

---

### CoreMemoryLog

A **lightweight, structured, in-memory logger** for runtime diagnostics.

- Captures logs without writing to disk.  
- Stores a rolling buffer of entries, accessible via `GetLogs()`.  
- Provides structured log entries with timestamp, level, caller method, and optional exception.  
- Can be adapted into `Debugger` for visualization, or consumed directly in tests.

**Example:**
```csharp
var logger = new InMemoryLogger();
logger.Log(LogLevel.Info, "Starting job {0}", jobId);

// Later
foreach (var entry in logger.GetLogs())
    Console.WriteLine($"{entry.Timestamp}: {entry.Message}");
```

---

## Project Overview

| Module                      | Purpose / Description                           |
| --------------------------- | ----------------------------------------------- |
| `CommonLibraryTests`        | Unit tests for common libraries                 |
| `CommonExtendedObjectsTests`| Unit tests for ExtendedSystemObjects            |
| `Imaging`                   | Image manipulation tools                        |
| `CommonControls`            | Shared WPF custom controls                      |
| `SQLiteHelper`              | SQLite DB interaction & abstraction             |
| `Mathematics`               | Math utilities and calculation libraries        |
| `CommonLibrarySqlLiteTests` | Tests for SQLite-related features               |
| `ExtendedSystemObjects`     | Extensions for .NET base classes                |
| `FileHandler`               | File system abstraction (copy, move, etc.)      |
| `RenderEngine`              | Experimental 2D/3D rendering backend            |
| `RenderEngineTests`         | All Tests related to rendering                  |
| `SQLiteGui`                 | UI frontend for SQLiteHelper                    |
| `Debugger`                  | Debugging framework + log viewer integration    |
| `ImageCompare`              | Pixel-based image comparison utility            |
| `CommonDialogs`             | Prebuilt WPF dialogs (folders, login, etc.)     |
| `Interpreter`               | Text command processor and parser               |
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
| `InterOp`                   | Useful bindings to Windows libraries            |
| `CoreInject`                | Minimal DI/injection support                    |
| `CoreConsole`               | Quick Console runner for applications           |
| `CoreMemoryLog`             | In-memory logger, structured, UI-ready          |
| `Plugin`                    | Base classes/interfaces for plugin system       |
| `ViewModel`                 | ViewModel bindings for WPF MVVM support         |
| `CoreWorker`                | Worker service abstraction                      |
