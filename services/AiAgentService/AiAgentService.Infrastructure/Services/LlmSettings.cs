namespace AiAgentService.Infrastructure.Services;

public class OllamaSettings
{
    public const string SectionName = "Ollama";
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string ChatModel { get; set; } = "llama3.2:1b";
    public string EmbeddingModel { get; set; } = "nomic-embed-text";
    public int EmbeddingDimensions { get; set; } = 768;
}

public class LlmSettings
{
    public const string SectionName = "Llm";
    /// <summary>Auto | Ollama | OpenAi | Mock</summary>
    public string Provider { get; set; } = "Auto";
}
