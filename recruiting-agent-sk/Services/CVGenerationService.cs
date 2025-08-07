using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents.AzureAI;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace RecruitingAgentSk.Services;

public class CVGenerationService
{
    private readonly AzureAIAgent _agent;
    private readonly string _promptTemplate;

    public CVGenerationService(AzureAIAgent agent)
    {
        _agent = agent;
        _promptTemplate = LoadEmbeddedPromptTemplate();
    }

    /// <summary>
    /// Generate a CV based on job profile and optionally a specific job offer
    /// </summary>
    /// <param name="jobProfile">The candidate's profile (as JSON string or object)</param>
    /// <param name="jobOffer">Optional job offer to tailor the CV for (as JSON string or object)</param>
    /// <returns>Generated CV content as string</returns>
    public async Task<string> GenerateCVAsync(object jobProfile, object? jobOffer = null)
    {
        var prompt = BuildPrompt(jobProfile?.ToString() ?? string.Empty, jobOffer?.ToString() ?? string.Empty);
        
        // Create a new thread for CV generation
        var cvThread = new AzureAIAgentThread(_agent.Client);
        
        try
        {
            var message = new ChatMessageContent(AuthorRole.User, prompt);
            var responses = new StringBuilder();
            
            await foreach (var response in _agent.InvokeAsync(message, cvThread))
            {
                responses.Append(response.Message.Content);
            }
            
            return responses.ToString();
        }
        finally
        {
            await cvThread.DeleteAsync();
        }
    }

    /// <summary>
    /// Generate a CV with JSON objects
    /// </summary>
    /// <param name="jobProfileJson">Job profile as JSON object</param>
    /// <param name="jobOfferJson">Optional job offer as JSON object</param>
    /// <returns>Generated CV content as string</returns>
    public async Task<string> GenerateCVFromJsonAsync(JsonElement jobProfileJson, JsonElement? jobOfferJson = null)
    {
        var jobProfileString = JsonSerializer.Serialize(jobProfileJson, new JsonSerializerOptions { WriteIndented = true });
        var jobOfferString = jobOfferJson.HasValue ? 
            JsonSerializer.Serialize(jobOfferJson.Value, new JsonSerializerOptions { WriteIndented = true }) : 
            string.Empty;

        return await GenerateCVAsync(jobProfileString, jobOfferString);
    }

    private string BuildPrompt(string jobProfile, string jobOffer)
    {
        var prompt = _promptTemplate;
        
        // Simple template replacement (since we're not using Handlebars here)
        prompt = prompt.Replace("{{jobProfile}}", jobProfile);
        
        if (!string.IsNullOrWhiteSpace(jobOffer))
        {
            prompt = prompt.Replace("{{#if jobOffer}}", "");
            prompt = prompt.Replace("{{/if}}", "");
            prompt = prompt.Replace("{{else}}", "<!--REMOVE_ELSE_BLOCK-->");
            prompt = prompt.Replace("{{jobOffer}}", jobOffer);
            
            // Remove else blocks
            var lines = prompt.Split('\n');
            var filteredLines = new List<string>();
            bool skipMode = false;
            
            foreach (var line in lines)
            {
                if (line.Trim() == "<!--REMOVE_ELSE_BLOCK-->")
                {
                    skipMode = true;
                    continue;
                }
                if (skipMode && line.Trim().StartsWith("{{#if"))
                {
                    skipMode = false;
                    continue;
                }
                if (!skipMode)
                {
                    filteredLines.Add(line);
                }
            }
            prompt = string.Join('\n', filteredLines);
        }
        else
        {
            // Remove if blocks, keep else blocks
            prompt = prompt.Replace("{{#if jobOffer}}", "<!--REMOVE_IF_BLOCK-->");
            prompt = prompt.Replace("{{else}}", "");
            prompt = prompt.Replace("{{/if}}", "");
            prompt = prompt.Replace("{{jobOffer}}", "");
            
            // Remove if blocks
            var lines = prompt.Split('\n');
            var filteredLines = new List<string>();
            bool skipMode = false;
            
            foreach (var line in lines)
            {
                if (line.Trim() == "<!--REMOVE_IF_BLOCK-->")
                {
                    skipMode = true;
                    continue;
                }
                if (skipMode && (line.Trim() == "" || line.Trim().StartsWith("**INSTRUCTIONS FOR GENERIC CV:**")))
                {
                    skipMode = false;
                    if (line.Trim().StartsWith("**INSTRUCTIONS FOR GENERIC CV:**"))
                    {
                        filteredLines.Add(line);
                    }
                    continue;
                }
                if (!skipMode)
                {
                    filteredLines.Add(line);
                }
            }
            prompt = string.Join('\n', filteredLines);
        }
        
        return prompt;
    }

    private string LoadEmbeddedPromptTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "recruiting_agent_sk.Prompts.GenerateCV.yaml";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            // Fallback: try to find the resource with different casing/naming
            var availableResources = assembly.GetManifestResourceNames();
            var cvResource = availableResources.FirstOrDefault(r => r.Contains("GenerateCV") || r.Contains("Prompts"));
            
            if (cvResource != null)
            {
                using var fallbackStream = assembly.GetManifestResourceStream(cvResource);
                using var fallbackReader = new StreamReader(fallbackStream!);
                return fallbackReader.ReadToEnd();
            }
            
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found. Available resources: {string.Join(", ", availableResources)}");
        }

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        
        // Extract just the template part from the YAML
        var templateStart = content.IndexOf("template: |");
        if (templateStart >= 0)
        {
            var templateContent = content.Substring(templateStart + "template: |".Length);
            // Remove the leading spaces from each line (YAML indentation)
            var lines = templateContent.Split('\n');
            var processedLines = lines.Select(line => 
                line.Length > 2 && line.StartsWith("  ") ? line.Substring(2) : line
            );
            return string.Join('\n', processedLines).Trim();
        }
        
        return content;
    }
}
