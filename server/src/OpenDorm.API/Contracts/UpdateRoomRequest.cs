using OpenDorm.Domain.Enums;

namespace OpenDorm.API.Contracts;

public record UpdateRoomRequest(
    string? Name = null,
    int? Capacity = null,
    Gender? Gender = null);