using FluentValidation;

namespace OpenDorm.Application.Features.Occupants.Commands.CheckOutOccupant;

public class CheckOutOccupantCommandValidator : AbstractValidator<CheckOutOccupantCommand>
{
    public CheckOutOccupantCommandValidator()
    {
        RuleFor(x => x.OccupantId)
            .NotEmpty().WithMessage("Идентификатор пользователя обязателен.");
    }
}