using MediatR;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryDetails;

public record GetDormitoryDetailsQuery(Guid Id) : IRequest<DormitoryDetailsDto>;