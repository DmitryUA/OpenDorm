using FluentValidation;

namespace OpenDorm.Application.Features.Users.Commands.RevokeRole;

public class RevokeRoleCommandValidator : AbstractValidator<RevokeRoleCommand>
{
    public RevokeRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Идентификатор пользователя обязателен.");

        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Имя роли обязательно.");
    }
}