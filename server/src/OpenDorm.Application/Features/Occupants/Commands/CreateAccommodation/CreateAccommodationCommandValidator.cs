using FluentValidation;

namespace OpenDorm.Application.Features.Occupants.Commands.CreateAccommodation;

public class CreateAccommodationCommandValidator : AbstractValidator<CreateAccommodationCommand>
{
    public CreateAccommodationCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Идентификатор комнаты обязателен.");

        RuleFor(x => x.OccupantId)
            .NotEmpty().WithMessage("Идентификатор жильца обязателен.");
    }
}