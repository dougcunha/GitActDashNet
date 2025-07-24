# AGENT.md - GitActDashNet

## Build/Test Commands
- **Build**: `dotnet build` (from GitActDashNet/ directory)
- **Run**: `dotnet run` (from GitActDashNet/ directory)
- **Restore**: `dotnet restore`
- **No tests currently**: Project has no test configuration or test files

## Architecture & Structure
- **Tech Stack**: .NET 9, Blazor Server, GitHub OAuth, Octokit.NET, Serilog
- **Main Project**: GitActDashNet/ (single project solution)
- **Key Directories**: Components/ (Blazor), Services/ (business logic), Utils/ (utilities), Data/ (models), Extensions/ (DI configuration)
- **Authentication**: GitHub OAuth with cookie-based sessions
- **Logging**: Serilog with structured logging to files and console

## Code Style & Conventions
- **Language**: C# 9+ with nullable reference types enabled, implicit usings, preview language version
- **Namespace**: GitActDashNet.{FolderName} (e.g., GitActDashNet.Services, GitActDashNet.Utils)
- **Error Handling**: Use OperationResult<T> pattern for service methods with Success/Warning/Failure states
- **Imports**: Blazor components use _Imports.razor, C# files use explicit using statements
- **Naming**: PascalCase for public members, camelCase for private fields, extension methods in Extensions/ folder
- **Services**: Register via extension methods in ServiceCollectionExtensions.cs, use dependency injection
- **Logging**: Use ILogger with structured logging, configure via LoggingExtensions
