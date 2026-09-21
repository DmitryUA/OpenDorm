using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenDorm.API.Contracts;
using OpenDorm.Application.Features.Occupants.Commands.CheckOutOccupant;
using OpenDorm.Application.Features.Occupants.Commands.CreateAccommodation;
using OpenDorm.Application.Features.Occupants.Commands.CreateOccupant;
using OpenDorm.Application.Features.Occupants.Commands.TransferOccupant;
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
    
    // POST: api/occupants/{occupant-id}/check-in
    [HttpPost("{id:guid}/check-in")]
    [SwaggerOperation("Создать новое заселение.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckIn(
        Guid id,
        [FromBody] CheckInOccupantRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateAccommodationCommand(request.RoomId, id);
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
    
    // POST: api/occupants/{id}/transfer
    [HttpPost("{id:guid}/transfer")]
    [SwaggerOperation(
        Summary = "Переселить жильца в другую комнату.",
        Description = "Принимает в теле запроса id комнаты в которую нужно переселить жильца. " +
                      "Вернёт идентификатор новго заселения.")
    ]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Transfer(
        Guid id,
        [FromBody] TransferOccupantRequest request,
        CancellationToken cancellationToken)
    {
        var command = new TransferOccupantCommand(id, request.TargetRoomId);
        var response = await mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }
}