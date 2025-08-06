
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
using SimpleAgentSk.Configuration;

// Build configuration following .NET best practices
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>() // Add user secrets for development
    .Build();

// Bind and validate configuration
var azureAIOptions = new AzureAIOptions();
configuration.GetSection(AzureAIOptions.SectionName).Bind(azureAIOptions);
azureAIOptions.Validate();

// Use implicit typing to avoid compile-time dependency on the concrete client type
PersistentAgentsClient agentsClient = AzureAIAgent.CreateAgentsClient(azureAIOptions.Endpoint, new DefaultAzureCredential());

// 1. Define an agent on the Azure AI agent service
PersistentAgent definition = await agentsClient.Administration.CreateAgentAsync(
    "gpt-4o",
    name: "Simple Agent with the infromation on Hungarian swimmers",
    description: "Simple hungarian swimmers agent",
    instructions: "You provide information about the Hungarian swimmers");

// 2. Create a Semantic Kernel agent based on the agent definition
AzureAIAgent agent = new(definition, agentsClient);

AzureAIAgentThread agentThread = new(agent.Client);
try
{
    ChatMessageContent message = new(AuthorRole.User, "Who was Alfred Hajos?");
    await foreach (ChatMessageContent response in agent.InvokeAsync(message, agentThread))
    {
        Console.WriteLine(response.Content);
    }
    message = new(AuthorRole.User, "What are latest achievements of the Hungarian swimmers?");
    await foreach (ChatMessageContent response in agent.InvokeAsync(message, agentThread))
    {
        Console.WriteLine(response.Content);
    }
}
finally
{
    await agentThread.DeleteAsync();
    await agentsClient.Administration.DeleteAgentAsync(definition.Id);
}
