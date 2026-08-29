using MediatR;

namespace OpenDorm.Application.Features.Dormitories.Commands.CreateDormitory;

public record CreateDormitoryCommand(
    string City,
    string Street,
    string House,
    int FloorCount) : IRequest<Guid>;