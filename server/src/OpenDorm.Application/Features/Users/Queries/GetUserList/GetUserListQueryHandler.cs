using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Application.Features.Users.Queries.Common;

namespace OpenDorm.Application.Features.Users.Queries.GetUserList;

public class GetUserListQueryHandler(IApplicationDbContext context) 
    : IRequestHandler<GetUserListQuery, IReadOnlyCollection<UserDto>>
{
    public async Task<IReadOnlyCollection<UserDto>> Handle(GetUserListQuery query, CancellationToken cancellationToken)
    {
        var rawData = await context.Users
            .AsNoTracking()
            .Select(u => new
            {
                u.Id,
                Login = u.Login.Value,
                LastName = u.LastName.Value,
                FirstName = u.FirstName.Value,
                Patronymic = u.Patronymic != null ? u.Patronymic.Value : null,
                u.IsActive,
                u.CreatedAt,
                Roles = context.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Join(
                        context.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => r.NormalizedName
                    )
                    .ToList()
            })
            .ToListAsync(cancellationToken);
        
        var sortedData = rawData
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToList();
        
        return
        [
            .. sortedData.Select(u => new UserDto(
                u.Id,
                u.Login,
                u.LastName,
                u.FirstName,
                u.Patronymic,
                u.IsActive,
                u.CreatedAt,
                u.Roles.AsReadOnly()
            ))
        ];
    }
}