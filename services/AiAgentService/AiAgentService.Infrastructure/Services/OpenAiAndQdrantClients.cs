using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AiAgentService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace AiAgentService.Infrastructure.Services;

public class QdrantVectorStore : IVectorStore
{
    private readonly QdrantClient _client;
    private readonly QdrantSettings _settings;
    private readonly ILogger<QdrantVectorStore> _logger;

    public QdrantVectorStore(IOptions<QdrantSettings> settings, ILogger<QdrantVectorStore> logger)
    {
        _settings = settings.Value;
        _client = new QdrantClient(_settings.Host, _settings.Port);
        _logger = logger;
    }

    public async Task EnsureCollectionAsync(CancellationToken cancellationToken = default)
    {
        var collections = await _client.ListCollectionsAsync(cancellationToken);
        if (collections.Contains(_settings.CollectionName)) return;

        await _client.CreateCollectionAsync(_settings.CollectionName, new VectorParams { Size = 1536, Distance = Distance.Cosine }, cancellationToken: cancellationToken);
        _logger.LogInformation("Created Qdrant collection {Collection}", _settings.CollectionName);
    }

    public async Task UpsertAsync(KnowledgeDocument doc, float[] embedding, CancellationToken cancellationToken = default)
    {
        var point = new PointStruct
        {
            Id = new PointId { Uuid = doc.Id },
            Vectors = embedding,
            Payload =
            {
                ["title"] = doc.Title,
                ["content"] = doc.Content,
                ["category"] = doc.Category
            }
        };

        await _client.UpsertAsync(_settings.CollectionName, [point], cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<KnowledgeDocument>> SearchAsync(float[] embedding, int limit, CancellationToken cancellationToken = default)
    {
        var results = await _client.SearchAsync(_settings.CollectionName, embedding, limit: (ulong)limit, cancellationToken: cancellationToken);
        return results.Select(r => new KnowledgeDocument
        {
            Id = r.Id.Uuid,
            Title = r.Payload["title"].StringValue,
            Content = r.Payload["content"].StringValue,
            Category = r.Payload["category"].StringValue
        }).ToList();
    }
}

public class OpenAiHttpClient : IOpenAiClient
{
    private readonly HttpClient _http;
    private readonly OpenAiSettings _settings;
    private readonly ILogger<OpenAiHttpClient> _logger;

    public OpenAiHttpClient(HttpClient http, IOptions<OpenAiSettings> settings, ILogger<OpenAiHttpClient> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
    }

    public async Task<float[]> CreateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            return MockEmbedding(text);

        var payload = new { model = _settings.EmbeddingModel, input = text };
        var response = await _http.PostAsJsonAsync("https://api.openai.com/v1/embeddings", payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        return json.GetProperty("data")[0].GetProperty("embedding").EnumerateArray().Select(x => x.GetSingle()).ToArray();
    }

    public async Task<(string Answer, int InputTokens, int OutputTokens)> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            _logger.LogWarning("OpenAI API key not configured, using mock response");
            return ($"[Mock AI] Based on context: {userPrompt[..Math.Min(200, userPrompt.Length)]}...", 100, 50);
        }

        var payload = new
        {
            model = _settings.Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        var response = await _http.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var answer = json.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        var usage = json.GetProperty("usage");
        return (answer, usage.GetProperty("prompt_tokens").GetInt32(), usage.GetProperty("completion_tokens").GetInt32());
    }

    private static float[] MockEmbedding(string text)
    {
        var hash = text.GetHashCode();
        var random = new Random(hash);
        return Enumerable.Range(0, 1536).Select(_ => (float)random.NextDouble()).ToArray();
    }
}
