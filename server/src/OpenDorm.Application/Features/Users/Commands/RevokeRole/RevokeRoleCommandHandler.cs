using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Users.Commands.RevokeRole;

public class RevokeRoleCommandHandler(
    IApplicationDbContext context,
    IUserRepository userRepository,
    IUnitOfWork uow) : IRequestHandler<RevokeRoleCommand>
{
    public async Task Handle(RevokeRoleCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), command.UserId);

        if (!Enum.TryParse<RoleName>(command.RoleName, ignoreCase: true, out var roleName))
            throw new NotFoundException(nameof(Role), command.RoleName);
        
        var roleId = await context.Roles
                         .AsNoTracking()
                         .Where(r => r.Name == roleName)
                         .Select(r => (Guid?)r.Id)
                         .FirstOrDefaultAsync(cancellationToken)
                     ?? throw new NotFoundException(nameof(Role), command.RoleName);
        
        user.RevokeRole(roleId);

        await uow.SaveChangesAsync(cancellationToken);
    }
}