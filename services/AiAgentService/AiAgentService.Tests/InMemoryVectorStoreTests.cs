using AiAgentService.Domain.Entities;
using AiAgentService.Infrastructure.Services;

namespace AiAgentService.Tests;

public class InMemoryVectorStoreTests
{
    [Fact]
    public async Task UpsertAndSearch_ReturnsMostSimilarDocument()
    {
        var store = new InMemoryVectorStore();
        var doc = new KnowledgeDocument { Id = "1", Title = "Users", Content = "Manage users" };
        var embedding = new float[] { 1f, 0f, 0f };

        await store.UpsertAsync(doc, embedding);
        var results = await store.SearchAsync(new float[] { 1f, 0f, 0f }, limit: 1);

        Assert.Single(results);
        Assert.Equal("Users", results[0].Title);
    }

    [Fact]
    public async Task Upsert_ReplacesExistingDocument()
    {
        var store = new InMemoryVectorStore();
        var id = "doc-1";
        await store.UpsertAsync(new KnowledgeDocument { Id = id, Title = "Old", Content = "Old" }, new float[] { 1f });
        await store.UpsertAsync(new KnowledgeDocument { Id = id, Title = "New", Content = "New" }, new float[] { 1f });

        var results = await store.SearchAsync(new float[] { 1f }, limit: 5);

        Assert.Single(results);
        Assert.Equal("New", results[0].Title);
    }

    [Fact]
    public async Task EnsureCollectionAsync_CompletesWithoutError()
    {
        var store = new InMemoryVectorStore();
        await store.EnsureCollectionAsync();
    }
}
