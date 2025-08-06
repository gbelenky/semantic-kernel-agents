# Recruiting Agent SK

This is a recruiting assistant Azure AI Agent application using Semantic Kernel.

The recruiting agent helps with:
- Candidate screening and evaluation
- Interview questions preparation
- Job requirement analysis
- Hiring best practices guidance
- Resume analysis and feedback

## Configuration

This application follows .NET configuration best practices with multiple configuration sources:

1. **appsettings.json** - Base configuration (committed to source control)
2. **appsettings.{Environment}.json** - Environment-specific configuration (Development, Production, etc.)
3. **User Secrets** - For sensitive development settings (recommended for local development)
4. **Environment Variables** - For runtime configuration (recommended for production)

### Configuration Priority (highest to lowest):
1. Environment Variables
2. User Secrets (Development only)
3. appsettings.{Environment}.json
4. appsettings.json

### Setup Options:

#### Option 1: Using Environment-Specific Files (Current)
The project includes `appsettings.Development.json` with the endpoint configuration.
Environment-specific files are excluded from source control for security.

#### Option 2: Using User Secrets (Recommended for Development)
```bash
dotnet user-secrets set "AzureAI:Endpoint" "https://your-ai-service.services.ai.azure.com/api/projects/your-project"
```

#### Option 3: Using Environment Variables
```bash
# Windows
set AzureAI__Endpoint=https://your-ai-service.services.ai.azure.com/api/projects/your-project

# Linux/macOS
export AzureAI__Endpoint=https://your-ai-service.services.ai.azure.com/api/projects/your-project
```

## Prerequisites

- .NET 9.0 SDK
- Azure credentials configured (Azure CLI, DefaultAzureCredential, etc.)
- Azure AI service endpoint configuration (see setup options above)

## Running

```bash
dotnet run
```

## Configuration Schema

```json
{
  "AzureAI": {
    "Endpoint": "https://your-ai-service.services.ai.azure.com/api/projects/your-project"
  }
}
```
