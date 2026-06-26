using System.Text.Json;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using Toka.Shared.Events;
using Toka.Shared.Messaging;

namespace UserService.Application.Services;

public class UserAppService : IUserAppService
{
    private readonly IUserRepository _repository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUserCache _cache;

    public UserAppService(IUserRepository repository, IEventPublisher eventPublisher, IUserCache cache)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _cache = cache;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cached = await _cache.GetListAsync(cancellationToken);
        if (cached is not null) return cached;

        var users = await _repository.GetAllAsync(cancellationToken);
        var dtos = users.Select(Map).ToList();
        await _cache.SetListAsync(dtos, cancellationToken);
        return dtos;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        return user is null ? null : Map(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Email, request.FirstName, request.LastName);

        if (await _repository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken) is not null)
            throw new InvalidOperationException("Email already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim().ToLowerInvariant(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim()
        };

        await _repository.AddAsync(user, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateAsync(cancellationToken);

        await _eventPublisher.PublishAsync("user.created", new UserCreatedEvent(user.Id, user.Email, user.FullName, DateTime.UtcNow), cancellationToken);
        return Map(user);
    }

    public async Task<UserDto?> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user is null) return null;

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.IsActive = request.IsActive;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.UpdateAsync(user, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateAsync(cancellationToken);

        await _eventPublisher.PublishAsync("user.updated", new UserUpdatedEvent(user.Id, user.Email, user.FullName, user.IsActive, DateTime.UtcNow), cancellationToken);
        return Map(user);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user is null) return false;

        await _repository.DeleteAsync(id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        await _cache.InvalidateAsync(cancellationToken);
        return true;
    }

    private static UserDto Map(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.FullName, user.IsActive, user.CreatedAtUtc);

    private static void ValidateRequest(string email, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("Invalid email.");
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("First and last name are required.");
    }
}

public interface IUserCache
{
    Task<IReadOnlyList<UserDto>?> GetListAsync(CancellationToken cancellationToken = default);
    Task SetListAsync(IReadOnlyList<UserDto> users, CancellationToken cancellationToken = default);
    Task InvalidateAsync(CancellationToken cancellationToken = default);
}
