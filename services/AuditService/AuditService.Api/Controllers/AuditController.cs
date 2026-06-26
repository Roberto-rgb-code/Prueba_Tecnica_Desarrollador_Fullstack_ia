using AuditService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditService.Api.Controllers;

[ApiController]
[Route("api/audit")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly IAuditQueryService _auditService;

    public AuditController(IAuditQueryService auditService) => _auditService = auditService;

    [HttpGet]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 100, CancellationToken cancellationToken = default) =>
        Ok(await _auditService.GetRecentAsync(limit, cancellationToken));

    [HttpGet("{resourceType}/{resourceId}")]
    public async Task<IActionResult> GetByResource(string resourceType, string resourceId, CancellationToken cancellationToken) =>
        Ok(await _auditService.GetByResourceAsync(resourceType, resourceId, cancellationToken));
}
