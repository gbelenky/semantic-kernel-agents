
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
using RecruitingAgentSk.Configuration;

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

// 1. Use existing agent from Azure AI Foundry (configured in appsettings)
PersistentAgent definition = await agentsClient.Administration.GetAgentAsync(azureAIOptions.AgentId);

// 2. Create a Semantic Kernel agent based on the existing agent definition
AzureAIAgent agent = new(definition, agentsClient);

AzureAIAgentThread agentThread = new(agent.Client);
try
{
    Console.WriteLine("=== Recruiting Assistant Agent ===");
    Console.WriteLine("Ask me anything about recruiting, interviewing, or hiring!");
    Console.WriteLine("Type 'exit' or 'quit' to end the conversation.\n");

    while (true)
    {
        Console.Write("You: ");
        string? userInput = Console.ReadLine();

        // Check for exit commands
        if (string.IsNullOrWhiteSpace(userInput) || 
            userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) || 
            userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("\nGoodbye! Thanks for using the Recruiting Assistant.");
            break;
        }

        Console.WriteLine("\nAgent:");
        ChatMessageContent message = new(AuthorRole.User, userInput);
        
        bool hasResponse = false;
        await foreach (ChatMessageContent response in agent.InvokeAsync(message, agentThread))
        {
            Console.WriteLine(response.Content);
            hasResponse = true;
        }

        if (!hasResponse)
        {
            Console.WriteLine("I didn't receive a response. Please try again.");
        }

        Console.WriteLine(); // Add blank line for readability
    }
}
finally
{
    await agentThread.DeleteAsync();
    // No need to delete the agent since we're using an existing one from Azure AI Foundry
}
