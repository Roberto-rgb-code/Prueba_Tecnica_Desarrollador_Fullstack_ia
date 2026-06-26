using AiAgentService.Application.DTOs;

namespace AiAgentService.Application.Interfaces;

public interface IAgentService
{
    Task<AgentQueryResponse> QueryAsync(AgentQueryRequest request, CancellationToken cancellationToken = default);
    Task SeedKnowledgeBaseAsync(CancellationToken cancellationToken = default);
}
