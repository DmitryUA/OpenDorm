using MediatR;
using OpenDorm.Application.Abstractions;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Exceptions;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork uow) : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.Id, cancellationToken)
                   ?? throw new NotFoundException(nameof(Domain.Aggregates.User.User), command.Id);

        if (command.LastName is not null && command.FirstName is not null)
        {
            user.UpdatePersonalData(
                new LastName(command.LastName),
                new FirstName(command.FirstName),
                command.Patronymic is null ? null : new Patronymic(command.Patronymic));
        }

        if (command.NewPassword is not null)
        {
            var newHash = passwordHasher.Hash(command.NewPassword);
            user.ChangePassword(newHash);
        }

        await uow.SaveChangesAsync(cancellationToken);
    }
}