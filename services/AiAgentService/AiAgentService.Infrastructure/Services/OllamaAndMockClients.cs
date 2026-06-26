using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiAgentService.Infrastructure.Services;

public class MockLlmClient : IOpenAiClient
{
    public string ProviderName => "Mock";

    public Task<float[]> CreateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var hash = text.GetHashCode();
        var random = new Random(hash);
        return Task.FromResult(Enumerable.Range(0, 768).Select(_ => (float)random.NextDouble()).ToArray());
    }

    public Task<(string Answer, int InputTokens, int OutputTokens)> CompleteAsync(
        string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var answer = MockAnswerBuilder.Build(userPrompt);
        var tokens = Math.Max(50, userPrompt.Length / 4);
        return Task.FromResult((answer, tokens, tokens / 2));
    }
}

public class OllamaHttpClient : IOpenAiClient
{
    private readonly HttpClient _http;
    private readonly OllamaSettings _settings;
    private readonly ILogger<OllamaHttpClient> _logger;

    public OllamaHttpClient(HttpClient http, IOptions<OllamaSettings> settings, ILogger<OllamaHttpClient> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
        if (_http.BaseAddress is null && !string.IsNullOrWhiteSpace(_settings.BaseUrl))
            _http.BaseAddress = new Uri(_settings.BaseUrl.TrimEnd('/') + "/");
        _http.Timeout = TimeSpan.FromMinutes(5);
    }

    public string ProviderName => "Ollama";

    public async Task<float[]> CreateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        var payload = new { model = _settings.EmbeddingModel, prompt = text };
        var response = await _http.PostAsJsonAsync("api/embeddings", payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        return json.GetProperty("embedding").EnumerateArray().Select(x => x.GetSingle()).ToArray();
    }

    public async Task<(string Answer, int InputTokens, int OutputTokens)> CompleteAsync(
        string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            model = _settings.ChatModel,
            stream = false,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            options = new { temperature = 0.2, num_predict = 512 }
        };

        _logger.LogInformation("Calling Ollama model {Model}", _settings.ChatModel);
        var response = await _http.PostAsJsonAsync("api/chat", payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var answer = json.GetProperty("message").GetProperty("content").GetString() ?? "";
        var inputTokens = json.TryGetProperty("prompt_eval_count", out var p) ? p.GetInt32() : userPrompt.Length / 4;
        var outputTokens = json.TryGetProperty("eval_count", out var e) ? e.GetInt32() : answer.Length / 4;
        return (answer.Trim(), inputTokens, outputTokens);
    }
}
