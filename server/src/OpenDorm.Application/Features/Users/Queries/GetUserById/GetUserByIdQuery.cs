using MediatR;
using OpenDorm.Application.Features.Users.Queries.Common;

namespace OpenDorm.Application.Features.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;