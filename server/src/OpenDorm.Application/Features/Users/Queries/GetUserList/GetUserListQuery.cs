using MediatR;
using OpenDorm.Application.Features.Users.Queries.Common;

namespace OpenDorm.Application.Features.Users.Queries.GetUserList;

public record GetUserListQuery : IRequest<IReadOnlyCollection<UserDto>>;