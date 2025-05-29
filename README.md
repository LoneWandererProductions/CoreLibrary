### General Utilities

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
  Some experimental work on custom Memory Management.

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

### Math Utilities

- **Matrix Calculation**  
  Basic matrix math: addition, multiplication, inversion, etc.

- **3D Vector Calculations**  
  Vector math used in rendering, physics, and pathfinding.

- **Fractals**  
  Tools for procedural generation of fractal imagery.

- **Statistics**  
  Calculate mean, median, variance, and basic statistical analysis.

### Coding & Framework Tools

- **Resource String Generator**  
  Auto-generates strongly typed string classes from raw resource files.

- **In-Memory Logger**  
  Lightweight logger that keeps runtime logs in memory.

- **Simple Dependency Injection**  
  Minimal DI container to inject and manage services across projects.

- **Worker Service Framework**  
  Manages background threads and long-running jobs with clean abstraction.

---

| Module                      | Purpose / Description                           |
| --------------------------- | ----------------------------------------------- |
| `CommonLibraryTests`        | Unit tests for common libraries                 |
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
| `Debugger`                  | Custom debugging tools with UI support          |
| `ImageCompare`              | Pixel-based image comparison utility            |
| `CommonDialogs`             | Prebuilt WPF dialogs (folders, login, etc.)     |
| `Interpreter`               | Text command processor and parser               |
| `CommonLibraryGuiTests`     | GUI testing for common components               |
| `Solaris`                   | 2D chessboard-style tile renderer               |
| `LightVector`               | Likely lightweight vector math tools            |
| `Serializer`                | XML serialization of data and structures        |
| `DataFormatter`             | CSV reading/writing and formatting              |
| `Communication`             | Some Network and webservice                     |
| `CoreBuilderTests`          | Tests for image/vector building tools           |
| `PluginLoader`              | Plugin management and loading system            |
| `CommonFilter`              | WPF control to filter data tables               |
| `CoreBuilder`               | Possibly utilities for image/vector creation    |
| `Pathfinder`                | A\* pathfinding for grid-based maps             |
| `InterOp`                   | Some useful bindings to Windows libraries.      |
| `CoreInject`                | Minimal DI/injection support                    |
| `CoreConsole`               | Quick Console to use some of my applications    |
| `CoreMemoryLog`             | In-memory runtime logger                        |
| `Plugin`                    | Base classes/interfaces for plugin system       |
| `ViewModel`                 | ViewModel bindings for WPF MVVM support         |
| `CoreWorker`                | Worker service abstraction                      |
