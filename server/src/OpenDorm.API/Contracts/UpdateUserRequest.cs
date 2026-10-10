namespace OpenDorm.API.Contracts;

public record UpdateUserRequest(
    Guid Id,
    string? LastName,
    string? FirstName,
    string? Patronymic,
    string? NewPassword);