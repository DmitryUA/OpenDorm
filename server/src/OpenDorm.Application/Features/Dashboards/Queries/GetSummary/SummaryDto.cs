namespace OpenDorm.Application.Features.Dashboards.Queries.GetSummary;

public record SummaryDto(
    int DormitoryCount,
    int ActiveRoomCount,
    int AccommodationCount,
    int ActiveOccupantCount);