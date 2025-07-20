# URL Configuration Guide

## Overview

This document explains how to configure custom URLs for the GitActDashNet application. This is particularly important for GitHub OAuth authentication, as the callback URL must match exactly what is configured in your GitHub OAuth app.

## Configuration Files

### appsettings.json and appsettings.Development.json

The application URL configuration is controlled through the `Urls` setting in your configuration files:

```json
{
  "Urls": "https://localhost:7160;http://localhost:5106"
}
```

### Multiple URLs

You can specify multiple URLs separated by semicolons:
- HTTPS URL: `https://localhost:7160`
- HTTP URL: `http://localhost:5106`

## GitHub OAuth Configuration

When setting up your GitHub OAuth app, make sure to configure the callback URL correctly:

1. **Homepage URL**: `https://localhost:7160` (or your custom URL)
2. **Authorization callback URL**: `https://localhost:7160/signin-github` (or your custom URL + `/signin-github`)

## Custom Port Configuration

To use a custom port, update the `Urls` setting in your `appsettings.Development.json` file:

```json
{
  "Urls": "https://localhost:8080;http://localhost:8081"
}
```

### Important Notes

- Always update your GitHub OAuth app configuration when changing URLs
- The callback path `/signin-github` is fixed and configured in the application
- HTTPS is recommended for production environments
- Both HTTP and HTTPS can be configured simultaneously

## Environment-Specific Configuration

### Development
Configure in `appsettings.Development.json`:
```json
{
  "Urls": "https://localhost:7160;http://localhost:5106"
}
```

### Production
Configure in `appsettings.json` or through environment variables:
```json
{
  "Urls": "https://yourdomain.com:443"
}
```

Or via environment variable:
```bash
export Urls="https://yourdomain.com:443"
```

## Troubleshooting

### Common Issues

1. **OAuth Callback Error**: Ensure the callback URL in GitHub matches your configured URL + `/signin-github`
2. **Port Already in Use**: Change the port number in the `Urls` configuration
3. **HTTPS Certificate Issues**: For development, ASP.NET Core generates a development certificate automatically

### Verification

After starting the application, check the logs for:
```
[HH:mm:ss INF] Using configured URLs: https://localhost:7160;http://localhost:5106
```

This confirms your URL configuration is being applied correctly.