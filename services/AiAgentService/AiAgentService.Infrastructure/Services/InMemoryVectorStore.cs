using AiAgentService.Domain.Entities;

namespace AiAgentService.Infrastructure.Services;

public class InMemoryVectorStore : IVectorStore
{
    private readonly List<(KnowledgeDocument Doc, float[] Embedding)> _store = [];

    public Task EnsureCollectionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task UpsertAsync(KnowledgeDocument doc, float[] embedding, CancellationToken cancellationToken = default)
    {
        _store.RemoveAll(x => x.Doc.Id == doc.Id);
        _store.Add((doc, embedding));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<KnowledgeDocument>> SearchAsync(float[] embedding, int limit, CancellationToken cancellationToken = default)
    {
        var results = _store
            .Select(x => (x.Doc, Score: CosineSimilarity(embedding, x.Embedding)))
            .OrderByDescending(x => x.Score)
            .Take(limit)
            .Select(x => x.Doc)
            .ToList();

        return Task.FromResult<IReadOnlyList<KnowledgeDocument>>(results);
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        var dot = 0f;
        var magA = 0f;
        var magB = 0f;
        for (var i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }
        return magA == 0 || magB == 0 ? 0 : dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB));
    }
}
