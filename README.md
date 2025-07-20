# GitActDashNet

A comprehensive GitHub Actions dashboard built with ASP.NET Core Blazor Server.

## Features

- GitHub OAuth authentication
- Dashboard for monitoring GitHub Actions
- Real-time workflow status updates
- Configurable server URLs and ports
- Structured logging with Serilog

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- GitHub OAuth App configured

### Configuration

#### GitHub OAuth Setup

1. Create a new OAuth App in your GitHub settings
2. Configure the following URLs (adjust ports as needed):
   - **Homepage URL**: `https://localhost:7160`
   - **Authorization callback URL**: `https://localhost:7160/signin-github`

#### Application Configuration

Update `appsettings.Development.json` with your GitHub OAuth credentials:

```json
{
  "GitHub": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  },
  "Urls": "https://localhost:7160;http://localhost:5106"
}
```

#### Custom Port Configuration

To run the application on custom ports, update the `Urls` setting in your configuration file:

```json
{
  "Urls": "https://localhost:8443;http://localhost:8080"
}
```

**Important**: When changing ports, ensure your GitHub OAuth app configuration matches the new URLs.

For detailed instructions, see [URL Configuration Guide](docs/URL-Configuration.md).

### Running the Application

```bash
cd GitActDashNet
dotnet run
```

The application will be available at the configured URLs (default: `https://localhost:7160`).

## Documentation

- [URL Configuration Guide](docs/URL-Configuration.md) - How to configure custom URLs and ports
- [Custom Port Example](docs/Custom-Port-Example.md) - Examples of using custom ports
- [Operation Result Pattern](docs/OPERATION_RESULT_PATTERN.md) - Error handling patterns
- [Logging Implementation](docs/Logging-Implementation.md) - Structured logging setup

## Project Structure

```
GitActDashNet/
├── Components/          # Blazor components
│   ├── Layout/         # Layout components
│   ├── Pages/          # Page components
│   └── Shared/         # Shared components
├── Data/               # Data models
├── Extensions/         # Extension methods
├── Services/           # Application services
├── Utils/              # Utility classes
└── wwwroot/           # Static web assets
```

## Technologies Used

- ASP.NET Core 9.0
- Blazor Server
- GitHub OAuth (AspNet.Security.OAuth.GitHub)
- Octokit.NET for GitHub API
- Serilog for structured logging

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License.