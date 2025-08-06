namespace RecruitingAgentSk.Configuration;

public class AzureAIOptions
{
    public const string SectionName = "AzureAI";
    
    public string Endpoint { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Endpoint))
        {
            throw new InvalidOperationException($"Configuration section '{SectionName}:Endpoint' is required but was not found or is empty.");
        }
        
        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException($"Configuration value '{SectionName}:Endpoint' must be a valid URI.");
        }

        if (string.IsNullOrWhiteSpace(AgentId))
        {
            throw new InvalidOperationException($"Configuration section '{SectionName}:AgentId' is required but was not found or is empty.");
        }
    }
}
