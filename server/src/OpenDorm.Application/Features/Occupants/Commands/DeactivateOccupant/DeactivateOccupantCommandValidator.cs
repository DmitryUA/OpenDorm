using FluentValidation;

namespace OpenDorm.Application.Features.Occupants.Commands.DeactivateOccupant;

public class DeactivateOccupantCommandValidator : AbstractValidator<DeactivateOccupantCommand>
{
    public DeactivateOccupantCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Идентификатор жильца обязателен.");
    }
}