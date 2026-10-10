using FluentValidation;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия обязательно.")
            .MinimumLength(LastName.MinLength)
            .WithMessage($"Фамилия должна быть не короче '{LastName.MinLength}' символов.")
            .MaximumLength(LastName.MaxLength)
            .WithMessage($"Фамилия должна не быть не длиннее '{LastName.MaxLength}' символов.")
            .Matches(LastName.LastNameRegex)
            .WithMessage("Фамилия должна содержать только буквы, и начинаться с заглавной буквы, остальные строчные");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя обязательно.")
            .MinimumLength(FirstName.MinLength)
            .WithMessage($"Имя должно быть не короче '{FirstName.MinLength}' символов.")
            .MaximumLength(FirstName.MaxLength)
            .WithMessage($"Имя должно быть не длиннее '{FirstName.MaxLength}' символов.")
            .Matches(FirstName.FirstNameRegex)
            .WithMessage("Имя должно содержать только буквы, и начинаться с заглавной буквы, остальные строчные.");

        RuleFor(x => x.Patronymic)
            .NotEmpty().WithMessage("Отчество не может быть пустой строкой, если оно указано.")
            .MinimumLength(Patronymic.MinLength)
            .WithMessage($"Отчество должно быть не короче '{Patronymic.MinLength}' символов.")
            .MaximumLength(Patronymic.MaxLength)
            .WithMessage($"Отчество должно быть не длиннее '{Patronymic.MaxLength}' символов.")
            .Matches(Patronymic.PatronymicRegex)
            .WithMessage("Отчество должно содержать только буквы и начинаться с заглавной буквы.")
            .When(x => x.Patronymic != null);

        RuleFor(x => x.NewPassword)
            .MinimumLength(6)
            .MaximumLength(100)
            .When(x => x.NewPassword != null);
    }
}