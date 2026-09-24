using FluentValidation;

namespace OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupants;

public class GetRoomOccupantsQueryValidator : AbstractValidator<GetRoomOccupantsQuery>
{
    public GetRoomOccupantsQueryValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("Идентификатор комнаты обязателен.");
    }
}