namespace OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryList;

public record DormitoryListDto(
    Guid Id,
    string City,
    string Street,
    string House,
    int FloorCount);