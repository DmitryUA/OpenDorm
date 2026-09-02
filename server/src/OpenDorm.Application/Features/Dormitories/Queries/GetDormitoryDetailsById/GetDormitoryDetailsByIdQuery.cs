using MediatR;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryDetailsById;

public record GetDormitoryDetailsByIdQuery(Guid Id) : IRequest<DormitoryDetailsDto>;