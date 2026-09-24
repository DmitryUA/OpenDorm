using FluentValidation;

namespace OpenDorm.Application.Features.Occupants.Commands.ActivateOccupant;

public class ActivateOccupantCommandValidator : AbstractValidator<ActivateOccupantCommand>
{
    public ActivateOccupantCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Идентификатор жильца обязателен.");
    }
}