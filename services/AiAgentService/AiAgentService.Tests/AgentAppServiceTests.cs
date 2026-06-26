using AiAgentService.Application.DTOs;
using AiAgentService.Domain.Entities;
using AiAgentService.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace AiAgentService.Tests;

public class AgentAppServiceTests
{
    private readonly Mock<IVectorStore> _vectorStore = new();
    private readonly Mock<IOpenAiClient> _openAi = new();
    private readonly Mock<IUserContextClient> _userContext = new();
    private readonly AgentAppService _service;

    public AgentAppServiceTests()
    {
        _userContext.Setup(c => c.GetUserSummaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("Total users: 2.");
        _openAi.Setup(o => o.CreateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new float[] { 0.1f, 0.2f, 0.3f });
        _openAi.Setup(o => o.CompleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("Respuesta de prueba", 120, 40));
        _vectorStore.Setup(v => v.SearchAsync(It.IsAny<float[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<KnowledgeDocument>
            {
                new() { Title = "Auth", Content = "Login with JWT" }
            });

        var settings = Options.Create(new OpenAiSettings { ApiKey = "test-key" });
        _service = new AgentAppService(
            _vectorStore.Object,
            _openAi.Object,
            _userContext.Object,
            settings,
            NullLogger<AgentAppService>.Instance);
    }

    [Fact]
    public async Task QueryAsync_ReturnsAnswerWithMetrics()
    {
        var result = await _service.QueryAsync(new AgentQueryRequest("How do I login?"));

        Assert.Contains("Respuesta de prueba", result.Answer);
        Assert.Single(result.Sources);
        Assert.Equal("Auth", result.Sources[0]);
        Assert.True(result.Metrics.LatencyMs >= 0);
        Assert.Equal(120, result.Metrics.InputTokens);
        Assert.Equal(40, result.Metrics.OutputTokens);
        Assert.True(result.Metrics.EstimatedCostUsd > 0);
    }

    [Fact]
    public async Task SeedKnowledgeBaseAsync_UpsertsAllDocuments()
    {
        _vectorStore.Setup(v => v.EnsureCollectionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _vectorStore.Setup(v => v.UpsertAsync(It.IsAny<KnowledgeDocument>(), It.IsAny<float[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.SeedKnowledgeBaseAsync();

        _vectorStore.Verify(v => v.UpsertAsync(It.IsAny<KnowledgeDocument>(), It.IsAny<float[]>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
    }
}
