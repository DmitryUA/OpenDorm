using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenDorm.Application.Abstractions;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Aggregates.User;
using OpenDorm.Domain.Enums;
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

        var roleName = Enum.Parse<RoleName>(command.RoleName, ignoreCase: true);
        
        var role = await context.Roles
                       .AsNoTracking()
                       .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken)
                   ?? throw new NotFoundException(nameof(Role), command.RoleName);

        var passwordHash = passwordHasher.Hash(command.Password);

        var user = new User(
            Guid.NewGuid(),
            login,
            passwordHash,
            new LastName(command.LastName),
            new FirstName(command.FirstName),
            command.Patronymic is null ? null : new Patronymic(command.Patronymic));

        user.AssignRole(role.Id);

        await userRepository.AddAsync(user, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}