using AuditService.Application.Interfaces;
using AuditService.Domain.Entities;
using AuditService.Domain.Repositories;
using Moq;

namespace AuditService.Tests;

public class AuditQueryServiceTests
{
    [Fact]
    public async Task GetRecentAsync_ReturnsMappedDtos()
    {
        var repo = new Mock<IAuditLogRepository>();
        repo.Setup(r => r.GetRecentAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditLog>
            {
                new() { Id = "1", EventType = "UserCreated", ServiceName = "UserService", Action = "Create", OccurredAtUtc = DateTime.UtcNow }
            });

        var service = new AuditQueryService(repo.Object);
        var result = await service.GetRecentAsync();

        Assert.Single(result);
        Assert.Equal("UserCreated", result[0].EventType);
    }

    [Fact]
    public async Task GetRecentAsync_RespectsLimit()
    {
        var repo = new Mock<IAuditLogRepository>();
        repo.Setup(r => r.GetRecentAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditLog>());

        var service = new AuditQueryService(repo.Object);
        await service.GetRecentAsync(5);

        repo.Verify(r => r.GetRecentAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByResourceAsync_ReturnsMappedDtos()
    {
        var repo = new Mock<IAuditLogRepository>();
        repo.Setup(r => r.GetByResourceAsync("User", "123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditLog>
            {
                new() { Id = "2", EventType = "UserUpdated", ServiceName = "UserService", Action = "Update", ResourceType = "User", ResourceId = "123", OccurredAtUtc = DateTime.UtcNow }
            });

        var service = new AuditQueryService(repo.Object);
        var result = await service.GetByResourceAsync("User", "123");

        Assert.Single(result);
        Assert.Equal("UserUpdated", result[0].EventType);
    }
}
