using MediatR;

namespace OpenDorm.Application.Features.Dormitories.Commands.UpdateDormitory;

public record UpdateDormitoryCommand(
    Guid DormitoryId,
    string City,
    string Street,
    string House) : IRequest;