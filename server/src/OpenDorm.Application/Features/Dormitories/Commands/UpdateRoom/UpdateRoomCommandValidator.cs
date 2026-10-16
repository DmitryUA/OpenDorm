using FluentValidation;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Dormitories.Commands.UpdateRoom;

public class UpdateRoomCommandValidator : AbstractValidator<UpdateRoomCommand>
{
    public UpdateRoomCommandValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Идентификатор комнаты обязателен.");
        
        RuleFor(x => x.DormitoryId)
            .NotEmpty().WithMessage("Идентификатор общежития обязателен.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Номер комнаты обязателен.")
            .MinimumLength(RoomName.MinLength)
            .WithMessage($"Номер комнаты не может быть короче {RoomName.MinLength} символов.")
            .MaximumLength(RoomName.MaxLength)
            .WithMessage($"Номер комнаты не может быть длинее {RoomName.MaxLength} символов.")
            .When(x => x.Name != null);

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Недопустимое значение пола.")
            .When(x => x.Gender != null);

        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(1).WithMessage("Вместимость комнаты должна быть больше 0.")
            .When(x => x.Capacity != null);
    }
}