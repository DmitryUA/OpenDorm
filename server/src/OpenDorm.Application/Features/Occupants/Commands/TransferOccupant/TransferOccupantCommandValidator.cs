using FluentValidation;

namespace OpenDorm.Application.Features.Occupants.Commands.TransferOccupant;

public class TransferOccupantCommandValidator : AbstractValidator<TransferOccupantCommand>
{
    public TransferOccupantCommandValidator()
    {
        RuleFor(x => x.OccupantId)
            .NotEmpty().WithMessage("Идентификатор жильца обязателен.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Идентификатор комнаты обязателен.");
    }
}