using MediatR;
using OpenDorm.Application.Abstractions.Persistence;
using OpenDorm.Domain.Abstractions;
using OpenDorm.Domain.Exceptions;

namespace OpenDorm.Application.Features.Users.Commands.ActivateUser;

public class ActivateUserCommandHandler(
    IUserRepository repository,
    IUnitOfWork uow) : IRequestHandler<ActivateUserCommand>
{
    public async Task Handle(ActivateUserCommand command, CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (user == null) throw new NotFoundException(nameof(Domain.Aggregates.User.User), command.Id);
        
        user.Activate();
        await uow.SaveChangesAsync(cancellationToken);
    }
}