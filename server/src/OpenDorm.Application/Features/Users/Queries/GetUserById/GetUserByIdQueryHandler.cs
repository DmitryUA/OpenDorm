using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Users.Queries.Common;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler(IApplicationDbContext context) : IRequestHandler<GetUserByIdQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await context.Users
                       .AsNoTracking()
                       .FirstOrDefaultAsync(u => u.Id == query.Id, cancellationToken)
                   ?? throw new NotFoundException(nameof(Domain.Aggregates.User.User), query.Id);
        
        var normalizedRoles = await context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == user.Id)
            .Join(
                context.Roles.AsNoTracking(),
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.NormalizedName)
            .ToListAsync(cancellationToken);

        return new UserDto(
            user.Id,
            user.Login.Value,
            user.LastName.Value,
            user.FirstName.Value,
            user.Patronymic?.Value,
            user.IsActive,
            user.CreatedAt,
            normalizedRoles.AsReadOnly());
    }
}