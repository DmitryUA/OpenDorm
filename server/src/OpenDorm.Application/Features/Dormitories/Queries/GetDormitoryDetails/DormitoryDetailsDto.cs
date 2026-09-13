namespace OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryDetails;

public record DormitoryDetailsDto(
    Guid Id,
    string City,
    string Street,
    string House,
    int TotalRoomCount,
    int TotalSeatCount,
    int TotalFloorCount,
    int TotalAvailablePlaceCount);