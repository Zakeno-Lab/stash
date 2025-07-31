# Stash - Windows Clipboard History Manager

<div align="center">
  <img src="synapse/logo_stash2.ico" alt="Stash Logo" width="128" height="128">
  
  **A modern, fast clipboard history manager for Windows**
  
  [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
  [![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download)
  [![Windows](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078d7.svg)](https://www.microsoft.com/windows)
</div>

## Features

### 🚀 Core Features
- **Clipboard History**: Automatically captures text, images, and URLs
- **Smart Search**: Progressive search with exact, fuzzy, and token matching
- **Global Hotkey**: Quick access with customizable keyboard shortcut
- **System Tray**: Runs silently in the background
- **Rich Metadata**: Tracks source application, window title, and timestamps
- **Content Detection**: Automatically identifies URLs, code snippets, and more

### 🎨 User Experience
- **Modern UI**: Clean, intuitive interface with WPF and Wpf-Ui theming
- **Grouped Display**: Items organized by date for easy browsing
- **Master-Detail View**: See full content and metadata at a glance
- **Application Icons**: Visual identification of source applications
- **Smooth Scrolling**: Optimized performance for large histories
- **Dark Mode Support**: Seamless integration with Windows theme

### ⚡ Performance
- **Instant Search**: Results appear as you type
- **Progressive Enhancement**: Automatic fuzzy matching when needed
- **Efficient Storage**: SQLite database with optimized queries
- **Memory Management**: Smart caching for icons and images
- **Batch Updates**: Smooth UI even with thousands of items

## Screenshots

<div align="center">
  <img src="docs/images/main-window.png" alt="Main Window" width="600">
  <br>
  <em>Main clipboard history window with search</em>
</div>

## Requirements

- Windows 10/11 (64-bit)
- .NET 8.0 Desktop Runtime (included in installer)
- ~50MB disk space

## Installation

### Option 1: Installer (Recommended)
1. Download the latest `Stash-Setup-*.exe` from [Releases](https://github.com/bkhtmm/stash/releases)
2. Run the installer
3. Optionally enable "Start with Windows"
4. Launch from Start Menu or Desktop shortcut

### Option 2: Build from Source
```bash
# Clone the repository
git clone https://github.com/bkhtmm/stash.git
cd stash

# Build the application
dotnet build synapse/synapse.csproj --configuration Release

# Run the application
dotnet run --project synapse/synapse.csproj
```

## Usage

1. **Start the Application**: Stash runs in the system tray
2. **Copy Content**: Use Ctrl+C as normal - Stash captures automatically
3. **View History**: Press the global hotkey (default: configurable)
4. **Search**: Start typing to search through your clipboard history
5. **Restore Item**: Click any item to copy it back to the clipboard

### Search Tips
- Type naturally - search adapts automatically
- Searches content, application names, and window titles
- Multi-word queries use intelligent token matching
- URL domains are searchable (e.g., "github")

## Development

### Prerequisites
- Visual Studio 2022 or VS Code
- .NET 8.0 SDK
- Windows 10/11 SDK

### Building
```bash
# Debug build
dotnet build synapse/synapse.csproj

# Release build  
dotnet build synapse/synapse.csproj --configuration Release

# Create installer (requires Inno Setup)
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer/synapse-setup.iss
```

### Architecture
- **MVVM Pattern**: Clean separation of concerns with CommunityToolkit.Mvvm
- **Dependency Injection**: Microsoft.Extensions.Hosting for service management
- **Event-Driven**: Custom EventBus for decoupled communication
- **Entity Framework Core**: Code-first SQLite database
- **WPF + Wpf-Ui**: Modern Windows desktop UI

For detailed architecture and development guidelines, see [CLAUDE.md](CLAUDE.md).

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Quick Start
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [WPF-UI](https://github.com/lepoco/wpfui) - Modern WPF theme library
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM framework
- [Entity Framework Core](https://github.com/dotnet/efcore) - Database ORM
- [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon) - System tray support
- [FuzzySharp](https://github.com/JakeBayer/FuzzySharp) - Fuzzy string matching

## Support

- **Issues**: [GitHub Issues](https://github.com/bkhtmm/stash/issues)
- **Discussions**: [GitHub Discussions](https://github.com/bkhtmm/stash/discussions)
- **Documentation**: [Wiki](https://github.com/bkhtmm/stash/wiki)

---

<div align="center">
  Made with ❤️ for the Windows community
</div>