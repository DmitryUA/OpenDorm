using FluentValidation;

namespace OpenDorm.Application.Features.Dormitories.Commands.DeactivateRoom;

public class DeactivateRoomCommandValidator : AbstractValidator<DeactivateRoomCommand>
{
    public DeactivateRoomCommandValidator()
    {
        RuleFor(x => x.DormitoryId)
            .NotEmpty().WithMessage("Идентифиикатор общежития обязателен.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Идентификатор комнаты обязателен.");
    }
}