# Recruiting Agent - Semantic Kernel

A sophisticated recruiting assistant built with Microsoft Semantic Kernel and Azure AI Agents that helps with recruiting tasks, interviewing guidance, and CV generation.

## Overview

This application creates an intelligent recruiting assistant that can:
- Answer questions about recruiting, interviewing, and hiring best practices
- Generate tailored CVs for candidates based on job requirements
- Simulate interviews with technical and professional skills questions
- Analyze candidate fit and identify strengths and gaps for specific roles
- Provide personalized recruiting guidance and advice
- Assist with job matching and application processes

## Architecture

The application uses a modern, clean architecture with the following key components:

### Core Technologies
- **.NET 9.0** - Latest .NET framework for high performance
- **Microsoft Semantic Kernel** - AI orchestration framework
- **Azure AI Agents** - Persistent AI agent capabilities
- **Azure Identity** - Secure authentication to Azure services
- **YAML Templates** - Simple, maintainable prompt templates

### Key Components

#### 1. Azure AI Integration
- Uses existing Azure AI Foundry agents (configured via `appsettings.json`)
- Leverages `DefaultAzureCredential` for secure, passwordless authentication
- Implements persistent agent threads for conversation continuity

#### 2. Recruiting Tools
- **CV Generation Function**: Custom kernel function for generating tailored CVs
- **Interview Simulation Function**: Generates technical and professional questions with candidate analysis
- YAML-based prompt templates for flexibility
- Embedded resource loading for reliable template access

#### 3. Configuration Management
- Environment-aware configuration (Development/Production)
- User secrets support for secure local development
- JSON-based settings with environment variable overrides

## Project Structure

```
recruiting-agent-sk/
├── Program.cs                          # Main application entry point
├── Configuration/
│   └── AzureAIOptions.cs              # Azure AI configuration model
├── Prompts/
│   ├── GenerateCV.yaml                # CV generation prompt template
│   └── InterviewSimulation.yaml      # Interview simulation prompt template
├── appsettings.json                   # Base configuration
├── appsettings.Development.json       # Development-specific settings
└── recruiting-agent-sk.csproj        # Project file with dependencies
```

## Setup and Configuration

### Prerequisites
- .NET 9.0 SDK
- Azure subscription with AI services
- Azure AI Foundry agent (configured and deployed)

### Configuration Steps

1. **Clone and Navigate**
   ```bash
   git clone <repository-url>
   cd recruiting-agent-sk
   ```

2. **Configure Azure AI Settings**
   
   Update `appsettings.json` or use user secrets:
   ```json
   {
     "AzureAI": {
       "Endpoint": "https://your-ai-foundry-endpoint.cognitiveservices.azure.com/",
       "AgentId": "asst_your_agent_id_here"
     }
   }
   ```

   For local development, use user secrets:
   ```bash
   dotnet user-secrets set "AzureAI:Endpoint" "https://your-endpoint.com/"
   dotnet user-secrets set "AzureAI:AgentId" "your-agent-id"
   ```

3. **Install Dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the Application**
   ```bash
   dotnet build
   ```

## Usage

### Running the Application
```bash
dotnet run
```

The application will start and display:
```
=== Recruiting Assistant Agent ===
Ask me anything about recruiting, interviewing, or hiring!
I can generate CVs for candidates and simulate interviews with technical and professional questions!
Available tools:
  • Generate CV: Create tailored resumes for job applications
  • Interview Simulation: Get interview questions and candidate fit analysis
Type 'exit' or 'quit' to end the conversation.

You: 
```

### Example Interactions

**General Recruiting Questions:**
```
You: What are the best practices for conducting technical interviews?

Agent: Here are some key best practices for technical interviews:
1. Prepare structured questions that assess both technical skills and problem-solving
2. Use real-world scenarios relevant to the role...
```

**CV Generation:**
```
You: Generate a CV for a senior software developer applying to Microsoft

Agent: I'll create a tailored CV for a senior software developer position at Microsoft.
[Generates detailed, customized CV content]
```

**Interview Simulation:**
```
You: Simulate an interview for a senior .NET developer position at a fintech company

Agent: I'll create a comprehensive interview simulation including:
- 3 technical questions about .NET, C#, and system design
- 3 professional skills questions about teamwork and problem-solving
- Analysis of candidate strengths and gaps
- Interview recommendations and fit score
[Generates complete interview preparation materials]
```

**Exit Commands:**
- Type `exit`, `quit`, or press Ctrl+C to end the conversation

## Code Architecture Deep Dive

### Main Application Flow (`Program.cs`)

1. **Configuration Building** (`BuildConfiguration()`)
   - Loads settings from multiple sources in priority order
   - Supports environment-specific configurations
   - Includes user secrets for secure local development

2. **Azure AI Client Setup**
   - Creates `PersistentAgentsClient` using DefaultAzureCredential
   - Retrieves existing agent definition from Azure AI Foundry
   - Instantiates `AzureAIAgent` with the retrieved definition

3. **Function Registration**
   - Loads CV generation and interview simulation functions from embedded YAML templates
   - Registers both functions as plugins in the agent's kernel under "RecruitingTools"

4. **Conversation Loop**
   - Creates persistent agent thread for conversation continuity
   - Handles user input and agent responses
   - Provides graceful exit handling

### Key Methods

#### `BuildConfiguration()`
```csharp
static IConfiguration BuildConfiguration()
{
    var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
    
    return new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddUserSecrets<Program>()
        .Build();
}
```
- Environment-aware configuration loading
- Multiple configuration sources with proper precedence
- Hot-reload support for development scenarios

#### `LoadCVGenerationFunction()`
```csharp
static KernelFunction LoadCVGenerationFunction()
{
    var assembly = Assembly.GetExecutingAssembly();
    var resourceName = "recruiting_agent_sk.Prompts.GenerateCV.yaml";
    
    using var stream = assembly.GetManifestResourceStream(resourceName);
    // Error handling and YAML content loading...
    
    return KernelFunctionFactory.CreateFromPrompt(yamlContent);
}
```
- Embedded resource loading for reliable CV template access
- Error handling for missing resources
- Creates semantic kernel function from YAML prompt

#### `LoadInterviewSimulationFunction()`
```csharp
static KernelFunction LoadInterviewSimulationFunction()
{
    var assembly = Assembly.GetExecutingAssembly();
    var resourceName = "recruiting_agent_sk.Prompts.InterviewSimulation.yaml";
    
    using var stream = assembly.GetManifestResourceStream(resourceName);
    // Error handling and YAML content loading...
    
    return KernelFunctionFactory.CreateFromPrompt(yamlContent);
}
```
- Embedded resource loading for reliable interview simulation template access
- Error handling for missing resources
- Creates semantic kernel function from YAML prompt for interview generation

## Dependencies

### Core Packages
- `Microsoft.SemanticKernel` (1.55.0) - AI orchestration framework
- `Microsoft.SemanticKernel.Agents.AzureAI` (1.55.0-preview) - Azure AI agent support
- `Azure.Identity` (1.14.0) - Azure authentication

### Configuration Packages
- `Microsoft.Extensions.Configuration` - Configuration framework
- `Microsoft.Extensions.Configuration.Json` - JSON configuration provider
- `Microsoft.Extensions.Configuration.EnvironmentVariables` - Environment variable provider
- `Microsoft.Extensions.Configuration.UserSecrets` - User secrets provider

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
dotnet user-secrets set "AzureAI:AgentId" "your-agent-id"
```

#### Option 3: Using Environment Variables
```bash
# Windows
set AzureAI__Endpoint=https://your-ai-service.services.ai.azure.com/api/projects/your-project
set AzureAI__AgentId=your-agent-id

# Linux/macOS
export AzureAI__Endpoint=https://your-ai-service.services.ai.azure.com/api/projects/your-project
export AzureAI__AgentId=your-agent-id
```

## Security Considerations

- **DefaultAzureCredential**: Uses Azure's recommended authentication flow
- **User Secrets**: Sensitive configuration kept out of source control
- **Environment Variables**: Production secrets managed through environment
- **No Hardcoded Secrets**: All sensitive data externalized

## Troubleshooting

### Common Issues

1. **Agent Not Found Error**
   - Verify `AgentId` in configuration
   - Ensure agent exists in Azure AI Foundry
   - Check endpoint URL format

2. **Authentication Errors**
   - Verify Azure credentials are properly configured
   - For local development, ensure you're logged into Azure CLI: `az login`
   - Check that your account has access to the AI services

3. **Embedded Resource Not Found**
   - Verify `GenerateCV.yaml` is marked as `EmbeddedResource` in project file
   - Check namespace matches project's root namespace

### Debug Mode
Set environment variable for detailed logging:
```bash
export DOTNET_ENVIRONMENT=Development
```

## Contributing

1. Follow existing code patterns and architecture
2. Add appropriate error handling and logging
3. Update this README for any architectural changes
4. Test thoroughly with different conversation scenarios

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
