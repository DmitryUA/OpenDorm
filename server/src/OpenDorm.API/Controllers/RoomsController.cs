using MediatR;
using Microsoft.AspNetCore.Mvc;
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
}