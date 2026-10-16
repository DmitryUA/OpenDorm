using FluentValidation;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Dormitories.Commands.UpdateDormitory;

public class UpdateDormitoryCommandValidator : AbstractValidator<UpdateDormitoryCommand>
{
    public UpdateDormitoryCommandValidator()
    {
        RuleFor(x => x.DormitoryId)
            .NotEmpty().WithMessage("Идентификатор общежития обязателен.");
        
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Город обязателен.")
            .MinimumLength(City.MinLength).WithMessage($"Название города не может быть короче {City.MinLength} символов.")
            .MaximumLength(City.MaxLength).WithMessage($"Название города не может быть длинее {City.MaxLength} символов.");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Улица обязательна.")
            .MinimumLength(Street.MinLength).WithMessage($"Название улицы не может быть короче {Street.MinLength} символов.")
            .MaximumLength(Street.MaxLength).WithMessage($"Название улицы не может быть длинее {Street.MaxLength} символов.");

        RuleFor(x => x.House)
            .NotEmpty().WithMessage("Номер дома обязателен.")
            .MaximumLength(HouseNumber.MaxLength)
            .WithMessage($"Номер дома не может быть длинее {HouseNumber.MaxLength} символов.");

    }
}