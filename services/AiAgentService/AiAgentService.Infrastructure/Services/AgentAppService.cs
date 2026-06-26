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
    public bool Enabled { get; set; }
    public int VectorSize { get; set; } = 768;
}

public class AgentAppService : IAgentService
{
    private const string SystemPrompt = """
        Eres Toka Assistant, agente del sistema Toka User Management.
        REGLAS:
        - Responde SIEMPRE en español.
        - Usa OBLIGATORIAMENTE la sección "CONTEXTO" del mensaje del usuario.
        - Si el contexto menciona roles, usuarios, auth o auditoría, inclúyelo en la respuesta.
        - Responde en 3-6 oraciones claras con viñetas si ayuda.
        - NO digas que falta información si el CONTEXTO ya tiene datos relevantes.
        """;

    private static string BuildUserPrompt(string question, string knowledgeContext, string liveData) =>
        $"""
        CONTEXTO (base de conocimiento RAG — DEBES usar esto):
        {knowledgeContext}

        DATOS EN VIVO DEL SISTEMA:
        {liveData}

        PREGUNTA DEL USUARIO: {question}

        Instrucción: Responde la pregunta usando el CONTEXTO de arriba. Si pregunta por roles, menciona Admin y User si aparecen en el contexto.
        """;

    private readonly IVectorStore _vectorStore;
    private readonly IOpenAiClient _llm;
    private readonly IUserContextClient _userContext;
    private readonly ILogger<AgentAppService> _logger;

    public AgentAppService(
        IVectorStore vectorStore,
        IOpenAiClient llm,
        IUserContextClient userContext,
        ILogger<AgentAppService> logger)
    {
        _vectorStore = vectorStore;
        _llm = llm;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<AgentQueryResponse> QueryAsync(AgentQueryRequest request, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var userContext = await _userContext.GetUserSummaryAsync(cancellationToken);
        var embedding = await _llm.CreateEmbeddingAsync(request.Question, cancellationToken);
        var sources = await _vectorStore.SearchAsync(embedding, limit: 4, cancellationToken);

        var context = sources.Count > 0
            ? string.Join("\n\n", sources.Select(s => $"• {s.Title}: {s.Content}"))
            : "Sin documentos recuperados.";
        var prompt = BuildUserPrompt(request.Question, context, userContext);

        var (answer, inputTokens, outputTokens) = await _llm.CompleteAsync(SystemPrompt, prompt, cancellationToken);

        if (_llm.ProviderName == "Ollama" && ShouldUseRagFallback(answer, prompt) && sources.Count > 0)
        {
            _logger.LogWarning("Ollama returned weak answer, applying RAG fallback synthesis");
            answer = MockAnswerBuilder.Build(prompt);
        }
        sw.Stop();

        var isLocal = _llm.ProviderName is "Mock" or "Ollama";
        var metrics = new AgentMetricsDto(
            sw.ElapsedMilliseconds,
            inputTokens,
            outputTokens,
            isLocal ? 0 : EstimateCost(inputTokens, outputTokens));

        _logger.LogInformation("Agent query via {Provider} in {LatencyMs}ms", _llm.ProviderName, metrics.LatencyMs);

        return new AgentQueryResponse(
            answer,
            sources.Select(s => s.Title).ToArray(),
            metrics,
            _llm.ProviderName == "Mock",
            _llm.ProviderName);
    }

    public async Task SeedKnowledgeBaseAsync(CancellationToken cancellationToken = default)
    {
        var docs = new[]
        {
            new KnowledgeDocument { Title = "User Management", Content = "Toka allows creating, updating, and deactivating users. Each user has email, first name, last name, and active status.", Category = "users" },
            new KnowledgeDocument { Title = "Authentication", Content = "Users authenticate via JWT tokens issued by Auth Service. Register at /api/auth/register and login at /api/auth/login.", Category = "auth" },
            new KnowledgeDocument { Title = "Roles", Content = "El sistema Toka incluye roles por defecto: Admin (acceso completo al sistema) y User (acceso estándar). Puedes listar roles en GET /api/roles, crear nuevos con POST /api/roles y asignar con POST /api/roles/{roleId}/assign/{userId}.", Category = "roles" },
            new KnowledgeDocument { Title = "Audit", Content = "All user and role events are published to RabbitMQ and stored in MongoDB audit logs accessible at /api/audit.", Category = "audit" }
        };

        await _vectorStore.EnsureCollectionAsync(cancellationToken);
        foreach (var doc in docs)
        {
            var embedding = await _llm.CreateEmbeddingAsync($"{doc.Title}: {doc.Content}", cancellationToken);
            await _vectorStore.UpsertAsync(doc, embedding, cancellationToken);
        }
    }

    private static decimal EstimateCost(int inputTokens, int outputTokens) =>
        inputTokens * 0.00000015m + outputTokens * 0.0000006m;

    private static bool ShouldUseRagFallback(string answer, string prompt)
    {
        if (string.IsNullOrWhiteSpace(answer))
            return true;

        var lower = answer.ToLowerInvariant();
        if (lower.Contains("no hay información")
            || lower.Contains("no hay suficiente")
            || lower.Contains("insufficient")
            || lower.Contains("no tengo información")
            || lower.Contains("no se proporcion")
            || lower.Contains("could you provide"))
            return true;

        // llama3.2:1b often echoes the prompt instead of answering
        if (lower.Contains("contexto (base de conocimiento")
            || lower.Contains("debes usar esto")
            || lower.Contains("datos en vivo del sistema")
            || lower.Contains("pregunta del usuario:"))
            return true;

        var normalizedAnswer = NormalizeForComparison(answer);
        var normalizedPrompt = NormalizeForComparison(prompt);
        if (normalizedAnswer.Length > 40
            && normalizedPrompt.Contains(normalizedAnswer[..Math.Min(normalizedAnswer.Length, 120)], StringComparison.Ordinal))
            return true;

        return false;
    }

    private static string NormalizeForComparison(string text) =>
        new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray()).ToLowerInvariant();
}

public interface IVectorStore
{
    Task EnsureCollectionAsync(CancellationToken cancellationToken = default);
    Task UpsertAsync(KnowledgeDocument doc, float[] embedding, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KnowledgeDocument>> SearchAsync(float[] embedding, int limit, CancellationToken cancellationToken = default);
}

public interface IOpenAiClient
{
    string ProviderName { get; }
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
