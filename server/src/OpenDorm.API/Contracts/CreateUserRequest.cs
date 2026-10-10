namespace OpenDorm.API.Contracts;

public record CreateUserRequest(
    string Login,
    string Password,
    string LastName,
    string FirstName,
    string? Patronymic,
    string RoleName);