using MediatR;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Users.Commands.DeactivateUser;

public class DeactivateUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork uow) : IRequestHandler<DeactivateUserCommand>
{
    public async Task Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(command.Id, cancellationToken)
                   ?? throw new NotFoundException(nameof(Domain.Aggregates.User.User), command.Id);

        user.Deactivate();
        await uow.SaveChangesAsync(cancellationToken);
    }
}