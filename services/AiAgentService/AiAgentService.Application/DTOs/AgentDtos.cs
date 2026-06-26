namespace AiAgentService.Application.DTOs;

public record AgentQueryRequest(string Question, string? UserId = null);
public record AgentQueryResponse(string Answer, string[] Sources, AgentMetricsDto Metrics);
public record AgentMetricsDto(long LatencyMs, int InputTokens, int OutputTokens, decimal EstimatedCostUsd);
public record SeedDocumentsRequest(string[] Documents);
