using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Enums;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Users.Commands.AssignRole;

public class AssignRoleCommandHandler(
    IApplicationDbContext context,
    IUserRepository userRepository,
    IUnitOfWork uow) : IRequestHandler<AssignRoleCommand>
{
    public async Task Handle(AssignRoleCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.UserId, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), command.UserId);
        
        if (!Enum.TryParse<RoleName>(command.RoleName, ignoreCase: true, out var roleName))
            throw new NotFoundException(nameof(Role), command.RoleName);
        
        var role = await context.Roles
                       .AsNoTracking()
                       .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken)
                   ?? throw new NotFoundException(nameof(Role), command.RoleName);
        
        user.AssignRole(role.Id);

        await uow.SaveChangesAsync(cancellationToken);
    }
}