using FluentValidation;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Dormitories.Commands.CreateDormitory;

public class CreateDormitoryCommandValidator : AbstractValidator<CreateDormitoryCommand>
{
    public CreateDormitoryCommandValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MinimumLength(City.MinLength).WithMessage($"City must be at least {City.MinLength} characters long.")
            .MaximumLength(City.MaxLength).WithMessage($"City must contain no more than {City.MaxLength} characters.");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.")
            .MinimumLength(Street.MinLength).WithMessage($"Street must be at least {Street.MinLength} characters long.")
            .MaximumLength(Street.MaxLength)
            .WithMessage($"Street must contain no more than {Street.MaxLength} characters.");

        RuleFor(x => x.House)
            .NotEmpty().WithMessage("House is required.")
            .MaximumLength(HouseNumber.MaxLength)
            .WithMessage($"House must contain no more than {HouseNumber.MaxLength} characters");

        RuleFor(x => x.FloorCount)
            .GreaterThanOrEqualTo(1).WithMessage("Floor count must be at least 1.");
    }
}