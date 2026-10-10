using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler(
    IApplicationDbContext context,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork uow) : IRequestHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var login = new Login(command.Login);

        if (await userRepository.ExistsByLoginAsync(login, cancellationToken))
            throw new DomainException($"User with login '{command.Login}' already exists.");

        var normalizedName = command.RoleName.ToUpperInvariant();
        
        var roleId = await context.Roles
                       .AsNoTracking()
                       .Where(r => r.NormalizedName == normalizedName)
                       .Select(r=>(Guid?)r.Id)
                       .FirstOrDefaultAsync(cancellationToken)
                   ?? throw new NotFoundException(nameof(Role), command.RoleName);

        var passwordHash = passwordHasher.Hash(command.Password);

        var user = new User(
            Guid.NewGuid(),
            login,
            passwordHash,
            new LastName(command.LastName),
            new FirstName(command.FirstName),
            command.Patronymic is null ? null : new Patronymic(command.Patronymic));

        user.AssignRole(roleId);

        await userRepository.AddAsync(user, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}