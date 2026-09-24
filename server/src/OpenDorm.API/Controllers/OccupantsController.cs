using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenDorm.API.Contracts;
using OpenDorm.Application.Abstractions;
using OpenDorm.Application.Common;
using OpenDorm.Application.Features.Occupants.Commands.ActivateOccupant;
using OpenDorm.Application.Features.Occupants.Commands.CheckOutOccupant;
using OpenDorm.Application.Features.Occupants.Commands.CreateAccommodation;
using OpenDorm.Application.Features.Occupants.Commands.CreateOccupant;
using OpenDorm.Application.Features.Occupants.Commands.DeactivateOccupant;
using OpenDorm.Application.Features.Occupants.Commands.TransferOccupant;
using OpenDorm.Application.Features.Occupants.Queries.GetOccupantAccommodationList;
using OpenDorm.Application.Features.Occupants.Queries.GetOccupantList;
using OpenDorm.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace OpenDorm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OccupantsController(IMediator mediator) : ControllerBase
{
    // GET: api/occupants
    [HttpGet]
    [SwaggerOperation(
        Summary = "Получить краткую информацию о жильцах.",
        Description = "Возвращает краткую информацию о жильцах. " +
                      "Поддерживает пагинацию. " +
                      "Поддерживает фильтры по: гендеру и статусу.")
    ]
    [ProducesResponseType(typeof(PagedResult<OccupantListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken,
        [FromQuery] bool? status = null,
        [FromQuery] Gender? gender = null,
        [FromQuery] int page = PagedQuery.DefaultPage,
        [FromQuery] int pageSize = PagedQuery.DefaultPageSize)
    {
        var query = new GetOccupantListQuery(gender, status, page, pageSize);
        var response = await mediator.Send(query, cancellationToken);

        return Ok(response);
    }
    
    // GET: api/occupants/{id}/accommodations
    [HttpGet("{id:guid}/accommodations")]
    [SwaggerOperation(
        Summary = "Получить историю заселений жильца.",
        Description = "Вернёт историю заселений жильца отсортированную по новых заселений к старым. " +
                      "Если дата выселения null значит комната используется жильцом в данный момент")
    ]
    [ProducesResponseType(typeof(IReadOnlyCollection<AccommodationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccommodationListByOccupantId(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetOccupantAccommodationListQuery(id);
        var response = await mediator.Send(query, cancellationToken);

        return Ok(response);
    }

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
    
    // POST: api/occupants/{id}/deactivate
    [HttpPost("{id:guid}/deactivate")]
    [SwaggerOperation("Сделать жильца неактивным.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateOccupantCommand(id);
        await mediator.Send(command, cancellationToken);

        return Ok();
    }
    
    // POST: api/occupants/{id}/activate
    [HttpPost("{id:guid}/activate")]
    [SwaggerOperation("Сделать жильца неактивным.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ActivateOccupantCommand(id);
        await mediator.Send(command, cancellationToken);

        return Ok();
    }
}