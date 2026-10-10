using FluentValidation;

namespace OpenDorm.Application.Features.Users.Commands.AssignRole;

public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Идентификатор пользователея обязателен.");

        RuleFor(x => x.RoleName)
            .NotEmpty().WithMessage("Имя роли обязательно.");
    }
}