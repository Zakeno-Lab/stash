# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Stash is a Windows clipboard history management application built with WPF and .NET 8. It runs as a system tray application that captures clipboard content and allows users to view/restore previous items through a hotkey-activated interface.

## Development Commands

### Build and Run
```bash
# Build the application
dotnet build synapse/synapse.csproj

# Run the application (starts minimized to system tray)
dotnet run --project synapse/synapse.csproj

# Build for release
dotnet build synapse/synapse.csproj --configuration Release
```

### Database Operations
```bash
# Add new migration (when modifying models)
dotnet ef migrations add <MigrationName> --project synapse

# Update database
dotnet ef database update --project synapse
```

## Architecture Overview

### Core Application Pattern
- **Hosting**: Uses Microsoft.Extensions.Hosting with dependency injection
- **Startup**: `ApplicationStartupManager` orchestrates initialization sequence
- **Services**: All major functionality implemented as injected services
- **MVVM**: CommunityToolkit.Mvvm for ViewModels with ObservableObject
- **Event-Driven**: Custom `EventBus` for inter-service communication

### Key Services (DI Container)
- `IClipboardService`: Monitors clipboard using Win32 APIs, handles deduplication and content type detection
- `IGlobalHotkeyService`: System-wide hotkey registration (NHotkey.Wpf)
- `ITrayIconService`: System tray icon and context menu
- `IDataService`: Database operations and clipboard item management
- `IApplicationService`: Core application lifecycle
- `IDatabaseInitializationManager`: EF Core SQLite setup
- `IEventBus`: Custom event system for inter-service communication
- `IWindowManager`: Window lifecycle and visibility management
- `IMainWindowService`: Main window creation and initialization
- `IApplicationIconService`: Application icon extraction and caching service
- `ISearchService`: Advanced search with multiple strategies (Exact, Fuzzy, Word Token, Hybrid)

### UI Architecture
- **Main Window**: `ClipboardHistoryWindow` (hidden by default, activated by hotkey)
- **Theme**: Wpf-Ui modern theme system
- **Layout**: Grouped listbox with master-detail, custom scrollbar styling
- **Grouping**: Items grouped by date using `DateGroupHelper`

### Data Layer
- **Database**: SQLite with Entity Framework Core
- **Location**: `%LocalAppData%\Synapse\synapse.db`
- **Models**: `ClipboardItem` with rich metadata including content, type, timestamp, source app, window title, executable path
- **Content Types**: Text, Image, URL, Code with type-specific metadata (word count, dimensions, domain, language detection)
- **Grouping**: Items grouped by date using `DateGroupDescription` and `CollectionViewSource`

## Important Implementation Details

### Service Registration
All services are registered in `ServiceRegistrationManager.cs` - add new services here and follow the existing pattern.

### Clipboard Integration
- Uses Win32 clipboard APIs through `IClipboardService` with `WM_CLIPBOARDUPDATE` message handling
- Supports text, image, and URL content types with automatic detection
- Implements sophisticated deduplication using content hashing and tracking
- Tracks source application and window title using foreground window detection
- Real-time monitoring with automatic UI updates via EventBus events
- Programmatic clipboard setting with history prevention using `SetContentWithoutHistory`

### UI Updates
- ViewModels subscribe to `ClipboardItemAddedEvent` for real-time updates
- Use `ObservableCollection` and `CollectionViewSource` for grouped display with date grouping
- Custom behaviors: `SmoothScrollBehavior`, `ScrollActivityBehavior`, `WindowBehavior`
- Search and content type filtering with instant UI updates
- Master-detail layout with metadata display in right panel
- Application icons displayed next to source app names using cached icon extraction
- Clickable URLs in detail view that open in default browser
- Improved search performance using ObservableCollection extensions for batch updates

### Database Schema
- Single `ClipboardItem` entity with computed properties for UI display
- Migrations managed explicitly (see Migrations/ folder)
- Recent migrations include metadata support, URL features, and ApplicationExecutablePath field
- Image files stored separately in `%LocalAppData%\Synapse\Images\` with paths in database
- Application icons cached in `%LocalAppData%\Synapse\AppIcons\` with memory and disk caching
- SQLite provider configured in `AppDbContext` with factory pattern for concurrency

## MVVM Principles and WPF Best Practices

### MVVM Architecture Guidelines
**STRICTLY FOLLOW** these MVVM principles when working on this project:

#### ViewModels
- Use `CommunityToolkit.Mvvm.ComponentModel.ObservableObject` as base class
- Use `[ObservableProperty]` attribute for simple properties with automatic change notification
- Use `[RelayCommand]` attribute for command methods
- Use `[NotifyPropertyChangedFor]` to trigger notifications for computed properties
- ViewModels should be registered as **transient** in DI to avoid state corruption
- **NEVER** reference Views directly from ViewModels
- **NEVER** put UI-specific logic in ViewModels (colors, visibility, etc.)
- Use data binding for all UI interactions - avoid code-behind event handlers

#### Views (XAML/Code-Behind)
- **NEVER** put business logic in code-behind
- Code-behind should only contain UI-specific logic (focus management, animations)
- Use data binding for all properties: `{Binding PropertyName}`
- Use command binding for all user interactions: `{Binding CommandName}`
- Use converters for UI-specific transformations
- Use behaviors for reusable UI interactions

#### Data Binding Best Practices
- Always specify `Mode=TwoWay` for input controls when needed
- Use `UpdateSourceTrigger=PropertyChanged` for real-time updates
- Use `StringFormat` for simple formatting instead of converters when possible
- Use `FallbackValue` and `TargetNullValue` for robust binding
- Prefer `{x:Static}` for static values over hardcoded strings

#### Commands
- Use `[RelayCommand]` for all user actions
- Command methods should be `void` or `async Task`
- Use command parameters for contextual data: `CommandParameter="{Binding Item}"`
- Implement `CanExecute` logic in separate methods when needed
- **NEVER** call commands directly from code-behind

#### Dependency Injection
- **ALL** dependencies must be injected through constructor
- Use interfaces for all service dependencies
- Register ViewModels as `Transient` to ensure fresh state
- Register Services as `Singleton` unless stateful behavior is needed
- Use `IDbContextFactory<T>` pattern for Entity Framework contexts

#### Event Communication
- Use the `EventBus` pattern for cross-service communication
- Create specific event classes in `Models/Events/` folder
- Subscribe to events in ViewModel constructors
- **ALWAYS** implement `IDisposable` and unsubscribe from events
- Use `Dispatcher.BeginInvoke()` for UI thread marshaling

#### Resource Management
- **ALWAYS** implement `IDisposable` in ViewModels that subscribe to events
- Dispose of event subscriptions in `Dispose()` method
- Use `using` statements for DbContext instances
- Dispose of file streams and other unmanaged resources

#### UI Patterns
- Use `CollectionViewSource` for grouping and sorting collections
- Use `ObservableCollection<T>` for dynamic collections
- Implement filtering through ViewModel properties, not UI manipulation
- Use `DateGroupDescription` for date-based grouping
- Use behaviors for complex UI interactions (scrolling, window management)

### WPF Specific Best Practices

#### Performance
- Use `VirtualizingStackPanel` for large lists
- Set `ScrollViewer.CanContentScroll="False"` for smooth scrolling
- Use `UpdateSourceTrigger=PropertyChanged` judiciously (can impact performance)
- Implement proper disposal patterns to prevent memory leaks

#### Styling and Theming
- Use Wpf-Ui dynamic resource brushes: `{DynamicResource AccentFillColorSecondaryBrush}`
- Define styles in ResourceDictionaries, not inline
- Use `TargetType` for implicit styles
- Use triggers for state-based styling instead of code-behind
- Follow Wpf-Ui naming conventions for consistent theming

#### Converters
- Create converters in `Converters/` folder
- Make converters stateless and thread-safe
- Use `IMultiValueConverter` for complex binding scenarios
- Register converters as static resources

#### Behaviors
- Use `Microsoft.Xaml.Behaviors.Wpf` for reusable UI behaviors
- Create behaviors in `Utils/` folder
- Make behaviors configurable through dependency properties
- Use behaviors for: window management, scroll handling, keyboard shortcuts

#### Error Handling
- Use try-catch blocks in ViewModels for user-facing operations
- Log errors appropriately (Debug.WriteLine for development)
- Never let exceptions bubble up to the UI thread unhandled
- Provide user feedback for error states through binding

### Code Organization
- **Services**: Business logic and data access (`Services/` folder)
- **ViewModels**: UI logic and state management (`ViewModels/` folder)
- **Views**: XAML UI definitions (`Views/` folder)
- **Models**: Data models and events (`Models/` folder)
- **Utils**: Helper classes and behaviors (`Utils/` folder)
- **Converters**: Value converters (`Converters/` folder)

## Recent Features and Improvements

### Code Content Type Detection (July 2025)
- Added automatic detection of code snippets in clipboard content
- `CodeDetector` utility identifies programming code using:
  - Keyword matching across multiple languages
  - Code pattern detection (brackets, operators, function calls)
  - Consistent indentation detection
  - Language identification for C++, C#, Java, Python, JavaScript, SQL, PHP, Go, Rust
- Code items displayed with monospace font and disabled text wrapping
- `BooleanToTextWrappingConverter` for proper code display formatting

### Application Icon Display (July 2025)
- Added `ApplicationExecutablePath` field to track source application's executable
- `ApplicationIconService` extracts and caches application icons:
  - Memory cache for fast access
  - Disk cache in `%LocalAppData%\Synapse\AppIcons\` for persistence
  - Thread-safe icon extraction with semaphore locking
  - Automatic cache cleanup support
- Icons displayed in main list view next to application names
- `ExecutablePathToIconConverter` for XAML binding support

### Clickable URLs in Detail View (July 2025)
- URLs in detail panel are now clickable hyperlinks
- Opens URLs in default browser using `OpenUrlCommand`
- Enhanced user experience for URL clipboard items

### Hybrid Search Strategy (July 2025)
- New unified search mode combining all search strategies
- Intelligently prioritizes exact matches while falling back to fuzzy/token matching
- Provides best overall search experience with automatic strategy selection
- Maintains high performance through optimized scoring algorithms

### Search Performance Improvements (July 2025)
- Added `ObservableCollectionExtensions.ReplaceAll()` for batch collection updates
- Significantly improved search responsiveness with large datasets
- Reduced UI freezing during search operations
- Maintains smooth scrolling and interaction during active searches

## Development Notes

### Adding New Features
1. Create service interface and implementation
2. Register in `ServiceRegistrationManager.cs` following existing patterns
3. Use EventBus for communication between services with custom event classes
4. Follow MVVM pattern with CommunityToolkit.Mvvm attributes (`[ObservableProperty]`, `[RelayCommand]`)
5. Use Wpf-Ui controls for consistent theming with dynamic resource brushes
6. Add migrations for database schema changes using `dotnet ef migrations add`
7. Use utility classes for common operations (`UrlDetector`, `ContentMetricsCalculator`, etc.)

### Testing
No test framework is currently configured. When adding tests, consider using xUnit with WPF test host.

### Hotkey System
Global hotkeys managed through `IGlobalHotkeyService` using NHotkey.Wpf. Default activation hotkey shows the main window.

### Key Utilities
- `UrlDetector`: URL validation and domain extraction
- `ContentMetricsCalculator`: Text and image metrics calculation
- `DateGroupHelper`: Date-based grouping for UI
- `WindowBehavior`: Window lifecycle management (hide on deactivate/escape)
- Custom scrollbar styling with Wpf-Ui theming integration
- `TextHighlightBehavior`: Dynamic text highlighting with search mode support
- `CodeDetector`: Code content detection with language identification (C++, C#, Java, Python, JavaScript, SQL, PHP, Go, Rust)
- `IconHelper`: Win32 API wrapper for icon extraction from executables
- `ObservableCollectionExtensions`: Batch update operations for improved performance
- `ExecutablePathToIconConverter`: XAML converter for displaying application icons

### Search Features
- **Progressive Search**: Automatic two-phase search system
  - **Phase 1**: Immediate exact matching (0-50ms) - shows results instantly
  - **Phase 2**: Smart enhancement with fuzzy/token matching (50-200ms) - runs automatically when needed
  - No user configuration required - search automatically adapts based on results
- **Search Service Architecture**:
  - `IProgressiveSearchService`: Manages two-phase search with automatic enhancement detection
  - `ISearchService`: Core search interface with multiple strategy implementations
  - Searches across content, source application, window title, and URL domain
  - Intelligent result merging prevents duplicates while maintaining relevance
- **Enhancement Logic**: 
  - Skip enhancement for queries < 3 characters
  - Skip if 5+ exact matches found (good enough)
  - Enhance if 0-2 exact matches (need more results)
  - Enhance for multi-word queries (token search helps)
- **Performance Optimizations**:
  - Proper cancellation with CancellationToken
  - FuzzySearchStrategy reduced from 10+ to 2-3 algorithms
  - 300ms search debouncing
  - Batch UI updates with `ObservableCollectionExtensions.ReplaceAll()`
- **UI Features**:
  - Subtle progress indicator during enhancement phase
  - Real-time highlighting adapts to search results
  - No manual search mode selection needed

### Package Dependencies
- **CommunityToolkit.Mvvm**: MVVM framework with source generators
- **Wpf-Ui**: Modern UI controls and theming
- **NHotkey.Wpf**: Global hotkey registration
- **Hardcodet.NotifyIcon.Wpf**: System tray icon support
- **Microsoft.Xaml.Behaviors.Wpf**: Behavior system for UI interactions
- **FuzzySharp**: Fuzzy string matching library for approximate search

## Installer and Distribution

### Building the Installer (July 2025)
- **Inno Setup**: Professional Windows installer creation
- **Auto-start**: Registry entry for Windows startup (user-configurable)
- **Location**: Installer script at `installer/synapse-setup.iss`
- **Build Steps**:
  1. Run `dotnet publish` for release build
  2. Open `synapse-setup.iss` in Inno Setup Compiler
  3. Compile (F9) to create `dist/Stash-Setup-1.0.0.exe`
- **Features**:
  - Handles running instances gracefully
  - Optional desktop shortcut
  - Start menu integration
  - Clean uninstall with registry cleanup
  - Windows 10/11 64-bit requirement