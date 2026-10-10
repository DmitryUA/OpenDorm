using FluentValidation;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty()
            .MinimumLength(Login.MinLength).WithMessage($"Логин не может быть короче '{Login.MinLength}' символов.")
            .MaximumLength(Login.MaxLength).WithMessage($"Логин не может быть длинее '{Login.MaxLength}' символов.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Пароль не может быть короче 8 символов.")
            .MaximumLength(20).WithMessage("Пароль не может быть длинее 20 символов.");

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

        RuleFor(x => x.RoleName)
            .NotEmpty()
            .Must(name => Enum.TryParse<Domain.Enums.RoleName>(name, true, out _))
            .WithMessage(command => $"Несуществующая роль: '{command.RoleName}'.");
    }
}