using MediatR;
using Microsoft.AspNetCore.Authorization;
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
using OpenDorm.Application.Features.Occupants.Commands.UpdateOccupant;
using OpenDorm.Application.Features.Occupants.Queries.GetOccupantAccommodationList;
using OpenDorm.Application.Features.Occupants.Queries.GetOccupantList;
using OpenDorm.Domain.Enums;

namespace OpenDorm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OccupantsController(IMediator mediator) : ControllerBase
{
    // GET: api/occupants
    /// <summary>Получить краткую информацию о жильцах.</summary>
    /// <remarks>
    /// Возвращает краткую информацию о жильцах.
    /// Поддерживает пагинацию.
    /// Поддерживает точные фильтры по ФИО, гендеру и статусу.
    /// </remarks>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <param name="status">Фильтр по статусу.</param>
    /// <param name="gender">Фильтр по полу.</param>
    /// <param name="lastName">Фильтр по фамилии.</param>
    /// <param name="firstName">Фильтр по имени.</param>
    /// <param name="patronymic">Фильтр по отчеству.</param>
    /// <param name="page">Номер страницы (пагинация).</param>
    /// <param name="pageSize">Размер страницы (пагинация).</param>
    /// <response code="200">Список жильцов получен</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    [HttpGet]
    [Authorize(Policy = Permissions.Occupants.Read)]
    [ProducesResponseType(typeof(PagedResult<OccupantListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken,
        [FromQuery] bool? status = null,
        [FromQuery] Gender? gender = null,
        [FromQuery] int page = PagedQuery.DefaultPage,
        [FromQuery] int pageSize = PagedQuery.DefaultPageSize,
        [FromQuery] string? lastName = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? patronymic = null)
    {
        var query = new GetOccupantListQuery(gender, status, lastName, firstName, patronymic, page, pageSize);
        var response = await mediator.Send(query, cancellationToken);

        return Ok(response);
    }
    
    // GET: api/occupants/{id}/accommodations
    /// <summary>Получить историю заселений жильца.</summary>
    /// <remarks>
    /// Вернёт историю заселений жильца отсортированную по новых заселений к старым.
    /// Если дата выселения null значит комната используется жильцом в данный момент.
    /// </remarks>
    /// <param name="id">Идентификатор жильца.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="200">История заселений жильца получена</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Жилец не найден</response>
    [HttpGet("{id:guid}/accommodations")]
    [Authorize(Policy = Permissions.Accommodations.Read)]
    [ProducesResponseType(typeof(IReadOnlyCollection<AccommodationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    /// <summary>Создать нового жильца.</summary>
    /// <param name="request">Параметры для создания нового жильца.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="201">Жилец создан</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    [HttpPost]
    [Authorize(Policy = Permissions.Occupants.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    /// <summary>Создать новое заселение.</summary>
    /// <param name="id">Идентификатор жильца.</param>
    /// <param name="request">Параметры для создания заселения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="201">Заселение создано.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="403">Недостаточно прав.</response>
    /// <response code="404">Жилец не найден.</response>
    /// <response code="409">Конфликт.</response>
    [HttpPost("{id:guid}/check-in")]
    [Authorize(Policy = Permissions.Accommodations.CheckIn)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    /// <summary>Выселить жильца.</summary>
    /// <param name="id">Идентификатор жильца.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="204">Жилец выселен</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Жилец не найден</response>
    /// <response code="409">Конфликт.</response>
    [HttpPost("{id:guid}/check-out")]
    [Authorize(Policy = Permissions.Accommodations.CheckOut)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckOut(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new CheckOutOccupantCommand(id);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
    
    // POST: api/occupants/{id}/transfer
    /// <summary>Переселить жильца в другую комнату.</summary>
    /// <remarks>
    /// Принимает в теле запроса id комнаты в которую нужно переселить жильца.
    /// Вернёт идентификатор новго заселения.
    /// </remarks>
    /// <param name="id">Идентификатор жильца.</param>
    /// <param name="request">Параметры для переселения.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="201">Жилец переселён</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Жилец не найден</response>
    /// <response code="409">Конфликт.</response>
    [HttpPost("{id:guid}/transfer")]
    [Authorize(Policy = Permissions.Accommodations.Transfer)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    /// <summary>Сделать жильца неактивным.</summary>
    /// <param name="id">Идентификатор жильца.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="204">Жилец деактивирован.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="403">Недостаточно прав.</response>
    /// <response code="404">Жилец не найден.</response>
    /// <response code="409">Конфликт.</response>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = Permissions.Occupants.Deactivate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateOccupantCommand(id);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
    
    // POST: api/occupants/{id}/activate
    /// <summary>Сделать жильца неактивным.</summary>
    /// <param name="id">Идентификатор жильца.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="204">Жилец активирован</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Жилец не найден</response>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = Permissions.Occupants.Activate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ActivateOccupantCommand(id);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
    
    // PUT: api/occupants/{id}
    /// <summary>Обновить жильца.</summary>
    /// <param name="id">идентификатор жильца.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <param name="request">Данные для обновления.</param>
    /// <response code="204">Жилец обновлён</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Жилец не найден</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Occupants.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        CancellationToken cancellationToken,
        [FromBody] UpdateOccupantRequest request)
    {
        var command = new UpdateOccupantCommand(
            id,
            request.LastName,
            request.FirstName,
            request.Patronymic);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }
}