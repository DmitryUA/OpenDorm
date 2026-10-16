namespace OpenDorm.API.Contracts;

public record UpdateOccupantRequest(
    string LastName,
    string FirstName,
    string Patronymic);