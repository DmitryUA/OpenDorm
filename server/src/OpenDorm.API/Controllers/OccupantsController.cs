using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenDorm.API.Contracts;
using OpenDorm.Application.Features.Occupants.Commands.CheckOutOccupant;
using OpenDorm.Application.Features.Occupants.Commands.CreateAccommodation;
using OpenDorm.Application.Features.Occupants.Commands.CreateOccupant;
using Swashbuckle.AspNetCore.Annotations;

namespace OpenDorm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OccupantsController(IMediator mediator) : ControllerBase
{
    // POST: api/occupants
    [HttpPost]
    [SwaggerOperation("Создать новго жильца.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOccupantRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateOccupantCommand(
            request.LastName,
            request.FirstName,
            request.Patronymic,
            request.Gender,
            request.BirthDate);

        var occupantId = await mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, occupantId);
    }
    
    // POST: api/occupants/{occupant-id}/rooms/{room-id}/check-in
    [HttpPost("{occupant-id:guid}/rooms/{room-id:guid}/check-in")]
    [SwaggerOperation("Создать новое заселение.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckIn(
        [FromRoute(Name = "occupant-id")] Guid occupantId,
        [FromRoute(Name = "room-id")] Guid roomId,
        CancellationToken cancellationToken)
    {
        var command = new CreateAccommodationCommand(roomId, occupantId);
        var response = await mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }
    
    // POST: api/occupants/{id}/check-out
    [HttpPost("{id:guid}/check-out")]
    [SwaggerOperation("Выселить жильца.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckOut(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new CheckOutOccupantCommand(id);
        await mediator.Send(command, cancellationToken);

        return Ok();
    }
}