using AiAgentService.Application.DTOs;
using AiAgentService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AiAgentService.Api.Controllers;

[ApiController]
[Route("api/agent")]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;

    public AgentController(IAgentService agentService) => _agentService = agentService;

    [HttpPost("query")]
    public async Task<ActionResult<AgentQueryResponse>> Query([FromBody] AgentQueryRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { error = "Question is required." });

        return Ok(await _agentService.QueryAsync(request, cancellationToken));
    }

    [HttpPost("seed")]
    public async Task<IActionResult> Seed(CancellationToken cancellationToken)
    {
        await _agentService.SeedKnowledgeBaseAsync(cancellationToken);
        return Ok(new { message = "Knowledge base seeded." });
    }
}
