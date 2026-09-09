using FluentValidation;
using OpenDorm.Domain.ValueObjects;

namespace OpenDorm.Application.Features.Dormitories.Commands.CreateRoom;

public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
{
    public CreateRoomCommandValidator()
    {
        RuleFor(x => x.DormitoryId)
            .NotEmpty().WithMessage("Идентификатор общежития обязателен.");
            
        RuleFor(x => x.RoomName)
            .NotEmpty().WithMessage("Номер комнаты обязателен.")
            .MinimumLength(RoomName.MinLength)
            .WithMessage($"Номер комнаты не может быть короче {RoomName.MinLength} символов.")
            .MaximumLength(RoomName.MaxLength)
            .WithMessage($"Номер комнаты не может быть длинее {RoomName.MaxLength} символов.");

        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage("Недопустимое значение пола.");

        RuleFor(x => x.Capacity)
            .GreaterThanOrEqualTo(1).WithMessage("Вместимость комнаты должна быть больше 0.");

        RuleFor(x => x.FloorNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Номер этажа для комнаты должен быть больше 0.");
    }
}