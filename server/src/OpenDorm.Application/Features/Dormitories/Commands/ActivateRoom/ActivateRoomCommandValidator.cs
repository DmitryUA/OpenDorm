using FluentValidation;

namespace OpenDorm.Application.Features.Dormitories.Commands.ActivateRoom;

public class ActivateRoomCommandValidator : AbstractValidator<ActivateRoomCommand>
{
    public ActivateRoomCommandValidator()
    {
        RuleFor(x => x.DormitoryId)
            .NotEmpty().WithMessage("Идентификатор общежития обязателен.");

        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Идентификатор комнаты обязателен.");
    }
}