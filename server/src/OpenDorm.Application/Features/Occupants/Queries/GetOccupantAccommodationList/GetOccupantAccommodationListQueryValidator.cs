using FluentValidation;

namespace OpenDorm.Application.Features.Occupants.Queries.GetOccupantAccommodationList;

public class GetOccupantAccommodationListQueryValidator
    : AbstractValidator<GetOccupantAccommodationListQuery>
{
    public GetOccupantAccommodationListQueryValidator()
    {
        RuleFor(x => x.OccupantId)
            .NotEmpty().WithMessage("Идентификатор жильца обязателен.");
    }
}