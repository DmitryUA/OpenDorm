namespace OpenDorm.Application.Features.Users.Queries.Common;

public record UserDto(
    Guid Id,
    string Login,
    string LastName,
    string FirstName,
    string? Patronymic,
    bool IsActive,
    DateTime CreatedAt,
    IReadOnlyCollection<string> Roles);