namespace OpenDorm.API.Contracts;

public record CreateDormitoryRequest(
    string City,
    string Street,
    string House,
    int FloorCount);