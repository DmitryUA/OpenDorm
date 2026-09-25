using FluentValidation;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupancyHistory;

public class GetRoomOccupancyHistoryQueryValidator : AbstractValidator<GetRoomOccupancyHistoryQuery>
{
    public GetRoomOccupancyHistoryQueryValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Идентификатор комнаты обязателен.");
        
        RuleFor(x => x.SortOrder)
            .IsInEnum().WithMessage("Некорректное значение сортировки.");
        
        RuleFor(x => x.StartCheckInDate)
            .LessThanOrEqualTo(x => x.EndCheckInDate)
            .When(x => x.StartCheckInDate.HasValue && x.EndCheckInDate.HasValue)
            .WithMessage("Дата начала не может быть позднее даты окончания.");
    }
}