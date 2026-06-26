using System.Diagnostics;
using System.Net.Http.Json;
using AiAgentService.Application.DTOs;
using AiAgentService.Application.Interfaces;
using AiAgentService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiAgentService.Infrastructure.Services;

public class OpenAiSettings
{
    public const string SectionName = "OpenAi";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
}

public class QdrantSettings
{
    public const string SectionName = "Qdrant";
    public string Host { get; set; } = "qdrant";
    public int Port { get; set; } = 6334;
    public string CollectionName { get; set; } = "toka_knowledge";
}

public class AgentAppService : IAgentService
{
    private const string SystemPrompt = """
        You are Toka Assistant, an AI agent for the Toka User Management system.
        Answer questions about users, roles, authentication, and audit using ONLY the provided context.
        If the context is insufficient, say so clearly. Be concise and professional.
        """;

    private readonly IVectorStore _vectorStore;
    private readonly IOpenAiClient _openAi;
    private readonly IUserContextClient _userContext;
    private readonly OpenAiSettings _openAiSettings;
    private readonly ILogger<AgentAppService> _logger;

    public AgentAppService(
        IVectorStore vectorStore,
        IOpenAiClient openAi,
        IUserContextClient userContext,
        IOptions<OpenAiSettings> openAiSettings,
        ILogger<AgentAppService> logger)
    {
        _vectorStore = vectorStore;
        _openAi = openAi;
        _userContext = userContext;
        _openAiSettings = openAiSettings.Value;
        _logger = logger;
    }

    public async Task<AgentQueryResponse> QueryAsync(AgentQueryRequest request, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var userContext = await _userContext.GetUserSummaryAsync(cancellationToken);
        var embedding = await _openAi.CreateEmbeddingAsync(request.Question, cancellationToken);
        var sources = await _vectorStore.SearchAsync(embedding, limit: 3, cancellationToken);

        var context = string.Join("\n\n", sources.Select(s => $"[{s.Title}]\n{s.Content}"));
        var prompt = $"""
            Context from knowledge base:
            {context}

            Live system data:
            {userContext}

            User question: {request.Question}
            """;

        var (answer, inputTokens, outputTokens) = await _openAi.CompleteAsync(SystemPrompt, prompt, cancellationToken);
        sw.Stop();

        var metrics = new AgentMetricsDto(
            sw.ElapsedMilliseconds,
            inputTokens,
            outputTokens,
            EstimateCost(inputTokens, outputTokens));

        _logger.LogInformation("Agent query completed in {LatencyMs}ms, tokens in={Input} out={Output}",
            metrics.LatencyMs, metrics.InputTokens, metrics.OutputTokens);

        return new AgentQueryResponse(
            answer,
            sources.Select(s => s.Title).ToArray(),
            metrics,
            string.IsNullOrWhiteSpace(_openAiSettings.ApiKey));
    }

    public async Task SeedKnowledgeBaseAsync(CancellationToken cancellationToken = default)
    {
        var docs = new[]
        {
            new KnowledgeDocument { Title = "User Management", Content = "Toka allows creating, updating, and deactivating users. Each user has email, first name, last name, and active status.", Category = "users" },
            new KnowledgeDocument { Title = "Authentication", Content = "Users authenticate via JWT tokens issued by Auth Service. Register at /api/auth/register and login at /api/auth/login.", Category = "auth" },
            new KnowledgeDocument { Title = "Roles", Content = "Roles Admin and User are seeded by default. Roles can be assigned to users via POST /api/roles/{roleId}/assign/{userId}.", Category = "roles" },
            new KnowledgeDocument { Title = "Audit", Content = "All user and role events are published to RabbitMQ and stored in MongoDB audit logs accessible at /api/audit.", Category = "audit" }
        };

        await _vectorStore.EnsureCollectionAsync(cancellationToken);
        foreach (var doc in docs)
        {
            var embedding = await _openAi.CreateEmbeddingAsync($"{doc.Title}: {doc.Content}", cancellationToken);
            await _vectorStore.UpsertAsync(doc, embedding, cancellationToken);
        }
    }

    private static decimal EstimateCost(int inputTokens, int outputTokens) =>
        inputTokens * 0.00000015m + outputTokens * 0.0000006m;
}

public interface IVectorStore
{
    Task EnsureCollectionAsync(CancellationToken cancellationToken = default);
    Task UpsertAsync(KnowledgeDocument doc, float[] embedding, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeDocument>> SearchAsync(float[] embedding, int limit, CancellationToken cancellationToken = default);
}

public interface IOpenAiClient
{
    Task<float[]> CreateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<(string Answer, int InputTokens, int OutputTokens)> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}

public interface IUserContextClient
{
    Task<string> GetUserSummaryAsync(CancellationToken cancellationToken = default);
}

public class HttpUserContextClient : IUserContextClient
{
    private readonly HttpClient _http;

    public HttpUserContextClient(HttpClient http) => _http = http;

    public async Task<string> GetUserSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _http.GetFromJsonAsync<List<UserSummary>>("api/users", cancellationToken);
            return users is null ? "No user data available." : $"Total users: {users.Count}. Sample: {string.Join(", ", users.Take(3).Select(u => u.Email))}";
        }
        catch
        {
            return "User service unavailable.";
        }
    }

    private record UserSummary(Guid Id, string Email, string FirstName, string LastName, string FullName, bool IsActive, DateTime CreatedAtUtc);
}
