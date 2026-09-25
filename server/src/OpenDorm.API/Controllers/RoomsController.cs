using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenDorm.Application.Common;
using OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupancyHistory;
using OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupants;
using Swashbuckle.AspNetCore.Annotations;

namespace OpenDorm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController(IMediator mediator) : ControllerBase
{
    // GET: api/rooms/{id}/occupants
    [HttpGet("{id:guid}/occupants")]
    [SwaggerOperation("Получить жильцов комнаты на данный момент.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<RoomOccupantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoomOccupants(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetRoomOccupantsQuery(id);
        var response = await mediator.Send(query, cancellationToken);

        return Ok(response);
    }
    
    // GET: api/rooms/{id}/history
    [HttpGet("{id:guid}/history")]
    [SwaggerOperation("Получить историю заселений для комнаты.")]
    [ProducesResponseType(typeof(PagedResult<RoomOccupancyHistoryListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistory(
        Guid id,
        CancellationToken cancellationToken,
        SortOrder sortOrder = SortOrder.Descending,
        DateOnly? startCheckInDate = null,
        DateOnly? endCheckInDate = null,
        int page = 1,
        int pageSize = 20)
    {
        var query = new GetRoomOccupancyHistoryQuery(id, sortOrder, startCheckInDate, endCheckInDate, page, pageSize);
        var response = await mediator.Send(query, cancellationToken);

        return Ok(response);
    }
}