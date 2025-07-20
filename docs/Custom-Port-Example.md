# Example Custom Port Configuration

This example shows how to configure the application to run on custom ports.

## Custom appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Urls": "https://localhost:8443;http://localhost:8080"
}
```

## GitHub OAuth App Configuration

When using custom ports, update your GitHub OAuth app settings:

- **Homepage URL**: `https://localhost:8443`
- **Authorization callback URL**: `https://localhost:8443/signin-github`

## Running the Application

1. Update your `appsettings.Development.json` with the desired URLs
2. Update your GitHub OAuth app configuration
3. Run the application: `dotnet run`
4. The application will be available at your configured URLs

## Environment Variables (Alternative)

Instead of modifying config files, you can use environment variables:

### PowerShell
```powershell
$env:Urls = "https://localhost:8443;http://localhost:8080"
dotnet run
```

### Command Prompt
```cmd
set Urls=https://localhost:8443;http://localhost:8080
dotnet run
```

### Linux/macOS
```bash
export Urls="https://localhost:8443;http://localhost:8080"
dotnet run
```