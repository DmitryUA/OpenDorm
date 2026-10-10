using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDorm.Application.Common;
using OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupancyHistory;
using OpenDorm.Application.Features.Dormitories.Queries.GetRoomOccupants;

namespace OpenDorm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomsController(IMediator mediator) : ControllerBase
{
    // GET: api/rooms/{id}/occupants
    /// <summary>Получить жильцов комнаты на данный момент.</summary>
    /// <param name="id">Идентификатор комнаты.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="200">Проживающие на данный момент в комнате получены</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Комната не найдена</response>
    [HttpGet("{id:guid}/occupants")]
    [Authorize(Policy = Permissions.Occupants.Read)]
    [ProducesResponseType(typeof(IReadOnlyCollection<RoomOccupantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    /// <summary>Получить историю заселений для комнаты.</summary>
    /// <param name="id">Идентификатор комнаты.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <param name="sortOrder">Тип сортировки.</param>
    /// <param name="startCheckInDate">Фильтр по периуду поиска (начало).</param>
    /// <param name="endCheckInDate">Фильтр по периуду поиска (конец).</param>
    /// <param name="page">Номер страницы (пагинация).</param>
    /// <param name="pageSize">Размер страницы (пагинация).</param>
    /// <response code="200">История заселений для комнаты получена</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Комната не найдена</response>
    [HttpGet("{id:guid}/history")]
    [Authorize(Policy = Permissions.Accommodations.Read)]
    [ProducesResponseType(typeof(PagedResult<RoomOccupancyHistoryListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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