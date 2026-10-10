using FluentValidation;

namespace OpenDorm.Application.Features.Users.Commands.ActivateUser;

public class ActivateUserCommandValidator : AbstractValidator<ActivateUserCommand>
{
    public ActivateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Иденификатор пользователя обязателен.");
    }
}