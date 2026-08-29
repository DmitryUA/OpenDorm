using MediatR;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryList;

public record GetDormitoryListQuery : IRequest<IReadOnlyCollection<DormitoryListDto>>;