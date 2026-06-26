namespace UserService.Application.DTOs;

public record UserDto(Guid Id, string Email, string FirstName, string LastName, string FullName, bool IsActive, DateTime CreatedAtUtc);
public record CreateUserRequest(string Email, string FirstName, string LastName);
public record UpdateUserRequest(string FirstName, string LastName, bool IsActive);
