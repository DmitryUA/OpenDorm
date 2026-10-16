namespace OpenDorm.API.Contracts;

public record UpdateDormitoryRequest(
    string City,
    string Street,
    string House);