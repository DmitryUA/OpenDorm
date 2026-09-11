namespace OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryDetails;

public record DormitoryDetailsDto(
    Guid Id,
    string City,
    string Street,
    string House,
    int TotalFloorCount,
    int TotalRoomCount,
    int TotalSeatCount,
    int TotalAvailablePlaceCount);