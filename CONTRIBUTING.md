# Contributing to Stash

First off, thank you for considering contributing to Stash! It's people like you that make Stash such a great tool.

## Code of Conduct

This project and everyone participating in it is governed by the [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check existing issues as you might find out that you don't need to create one. When you are creating a bug report, please include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Provide specific examples to demonstrate the steps**
- **Describe the behavior you observed and what behavior you expected**
- **Include screenshots if possible**
- **Include your environment details** (Windows version, .NET version, etc.)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide a detailed description of the suggested enhancement**
- **Provide specific examples to demonstrate the enhancement**
- **Describe the current behavior and expected behavior**
- **Explain why this enhancement would be useful**

### Pull Requests

1. Fork the repo and create your branch from `master`
2. If you've added code that should be tested, add tests
3. If you've changed APIs, update the documentation
4. Ensure the test suite passes
5. Make sure your code follows the existing code style
6. Issue that pull request!

## Development Setup

### Prerequisites

- Windows 10/11
- Visual Studio 2022 or VS Code
- .NET 8.0 SDK
- Git

### Setting up your development environment

1. Clone your fork:
   ```bash
   git clone https://github.com/YOUR-USERNAME/stash.git
   cd stash
   ```

2. Build the project:
   ```bash
   dotnet build synapse/synapse.csproj
   ```

3. Run the application:
   ```bash
   dotnet run --project synapse/synapse.csproj
   ```

### Development Guidelines

#### Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Keep methods small and focused
- Add XML documentation comments for public APIs
- Use async/await for asynchronous operations

#### MVVM Pattern

This project strictly follows the MVVM pattern:

- **ViewModels** must inherit from `ObservableObject`
- Use `[ObservableProperty]` for properties
- Use `[RelayCommand]` for commands
- Never reference Views from ViewModels
- Use data binding for all UI interactions

#### Database Changes

When modifying database models:

1. Make your changes to the model classes
2. Create a new migration:
   ```bash
   dotnet ef migrations add YourMigrationName --project synapse
   ```
3. Test the migration thoroughly
4. Include migration files in your PR

#### Testing

- Write unit tests for new functionality
- Ensure existing tests pass
- Test UI changes manually on Windows 10 and 11
- Test with different DPI settings

### Commit Messages

- Use the present tense ("Add feature" not "Added feature")
- Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit the first line to 72 characters or less
- Reference issues and pull requests liberally after the first line

### Documentation

- Update the README.md if needed
- Update CLAUDE.md for AI-assisted development changes
- Add XML documentation comments for public APIs
- Include inline comments for complex logic

## Project Structure

```
stash/
├── synapse/                 # Main application project
│   ├── Services/           # Business logic services
│   ├── ViewModels/         # MVVM ViewModels
│   ├── Views/              # WPF Views
│   ├── Models/             # Data models
│   ├── Utils/              # Utility classes
│   └── Converters/         # Value converters
├── installer/              # Inno Setup installer
└── docs/                   # Documentation
```

## Working with AI Assistants

This project includes a `CLAUDE.md` file that provides context for AI-assisted development. When using AI tools:

- Refer to CLAUDE.md for architectural decisions
- Follow the patterns established in the codebase
- Ensure AI-generated code follows our guidelines

## Community

- Join discussions in [GitHub Discussions](https://github.com/Zakeno-Lab/stash/discussions)
- Follow the project for updates
- Star the project if you find it useful!

## Recognition

Contributors will be recognized in:
- The project README
- Release notes for significant contributions
- Special thanks in the application About dialog

Thank you for contributing to Stash! 🎉