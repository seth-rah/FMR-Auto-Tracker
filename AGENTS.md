## Enterprise Coding Standards

### 1. Code Conventions

- Follow the existing code style and conventions used in the project.
- Use meaningful names for classes, methods, properties, and variables (e.g., `ReadGameState` instead of `rGS`).
- Avoid abbreviations unless they are universally known (e.g., use `Guid` instead of `gID`).
- **All public APIs require XML documentation** for IntelliSense support.
- Use `sealed` classes by default unless inheritance is explicitly required.
- Use `readonly` keyword for immutable fields and structs.

### 2. Architecture Layers

| Layer | Responsibility | namespace |
|-------|---------------|------------|
| **ProcessHook** | Low-level memory access, Win32 API | `YuGiOh_Forbidden_Memories_Monitor.ProcessHook` |
| **DataModel** | Immutable data structures | `YuGiOh_Forbidden_Memories_Monitor.DataModel` |
| **DataReader** | Read game state from memory | `YuGiOh_Forbidden_Memories_Monitor.DataReader` |
| **LogicEngine** | Business rules, scoring, calculations | `YuGiOh_Forbidden_Memories_Monitor.LogicEngine` |
| **UI Layer** | Presentation logic | `YuGiOh_Forbidden_Memories_Monitor` |

#### Interface Requirements

All core services MUST implement interfaces for testability and extensibility:

```
IDataReader      -> DataReader (implementation)
IProcessMonitor -> ProcessMonitor (implementation)
IScoreCalculator -> ScoreCalculator (implementation)
```

### 3. Dependency Injection & Extensibility

- Use constructor injection for dependencies.
- Accept interfaces in constructors, not concrete implementations.
- Support optional dependencies via nullable parameters.

```csharp
public DataReader(IntPtr processHandle, ulong ramBaseAddress)
    : this(processHandle, ramBaseAddress, new ScoreCalculator())
{
}

public DataReader(IntPtr processHandle, ulong ramBaseAddress, IScoreCalculator scoreCalculator)
{
    _scoreCalculator = scoreCalculator ?? throw new ArgumentNullException(nameof(scoreCalculator));
}
```

### 4. Thread Safety

- Use `ConcurrentDictionary` for thread-safe collections.
- Use `lock` objects for critical sections.
- Avoid static mutable state; use thread-local storage when necessary.
- Implement `IDisposable` for classes with `Timer` or native resources.

### 5. Data Model Patterns

#### Immutable Object with Builder

```csharp
public sealed class GameState
{
    public bool IsProcessAttached { get; }
    public string ProcessName { get; }
    
    private GameState(Builder builder) { /* ... */ }
    
    public Builder ToBuilder() => new(this);
    
    public sealed class Builder { /* ... */ }
}
```

### 6. Error Handling & Logging

- Log all errors to Windows Event Viewer with a clear identifier.
- Use structured logging with operation context.
- Wrap critical sections in try-catch with meaningful fallbacks.

### 7. Memory Reading

- Read memory every frame (~16ms) for real-time updates.
- Use buffer pooling for large reads.
- Validate all reads with byte count checks.

### 8. UI Refresh

- Use WPF `Dispatcher` for thread-safe UI updates.
- Update UI in a dedicated timer tick handler.
- Avoid direct UI updates from background threads.

## Project Structure

```
YuGiOh Forbidden Memories Monitor/
├── ProcessHook/
│   └── ProcessHook.cs          # Memory access APIs
├── DataModel/
│   ├── GameState.cs            # Immutable game state
│   └── MemoryMap.cs            # Memory address constants
├── DataReader/
│   ├── IDataReader.cs          # Interface
│   └── DataReader.cs           # Implementation
├── LogicEngine/
│   ├── IScoreCalculator.cs    # Interface
│   ├── ScoreCalculator.cs      # Implementation
│   └── ScoreTierRegistry.cs    # Score tier constants
├── IProcessMonitor.cs         # Interface
├── ProcessMonitor.cs          # Implementation
├── MainWindow.xaml.cs         # UI code-behind
├── ErrorLogger.cs             # Logging utilities
└── AGENTS.md                  # This file
```

## Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Interfaces | `I` + PascalCase | `IDataReader` |
| Implementations | PascalCase | `DataReader` |
| Constants | PascalCase | `PageReadWrite` |
| Static Readonly | PascalCase | `_scanLogs` |
| Properties | PascalCase | `IsProcessAttached` |
| Methods | PascalCase | `GetLastMemoryScanLog` |

## Extensibility Guidelines

### Adding New Games

1. Create memory address constants in `MemoryMap.cs`
2. Implement `IGameDetector` for game-specific detection
3. Add detection patterns to `ProcessHook.AutoDetectPS1RAMBase`

### Adding New Features

1. Define interface in appropriate namespace
2. Implement in concrete class
3. Document in AGENTS.md

### Testing

- Mock interfaces for unit testing.
- Use dependency injection for testability.
- Keep business logic in LogicEngine for isolated testing.

## Additional Notes

- Ensure all file paths are absolute and use consistent naming conventions.
- Do not commit files that likely contain secrets or keys (.env, credentials.json, etc.). Warn the user if they specifically request to commit those files.
- You are programming in a windows environment, restrict shell commands to ones that work within windows 11
- Build using Visual Studio 2026 directly on the system using a .NET 10 template.