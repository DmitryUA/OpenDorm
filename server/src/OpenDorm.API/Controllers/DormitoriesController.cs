using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDorm.API.Contracts;
using OpenDorm.Application.Abstractions;
using OpenDorm.Application.Common;
using OpenDorm.Application.Features.Dormitories.Commands.ActivateRoom;
using OpenDorm.Application.Features.Dormitories.Commands.CreateDormitory;
using OpenDorm.Application.Features.Dormitories.Commands.CreateRoom;
using OpenDorm.Application.Features.Dormitories.Commands.DeactivateRoom;
using OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryDetails;
using OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryList;
using OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryRoomsList;

namespace OpenDorm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DormitoriesController(IMediator mediator) : ControllerBase
{
    // GET: api/dormitories
    /// <summary>Получить список всех общежитий.</summary>
    /// <remarks>Возвращает краткую информацию по каждому общежитию.</remarks>
    /// <response code="200">Список общежитий получен.</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    [HttpGet]
    [Authorize(Policy = Permissions.Dormitories.Read)]
    [ProducesResponseType(typeof(IReadOnlyCollection<DormitoryListDto>),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetDormitoryListQuery();
        var response = await mediator.Send(query, cancellationToken);

        return Ok(response);
    }
    
    // GET: api/dormitories/{id}/details
    /// <summary>Получить детальную информацию об общежитии по идентификатору.</summary>
    /// <remarks>Вовзращает детальную информацию об общежитии по его идентификатору.</remarks>
    /// <param name="id">Идентификатор общежития.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="200">Информация об общежитии получена</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Общежитие не найдено</response>
    [HttpGet("{id:guid}/details")]
    [Authorize(Policy = Permissions.Dormitories.Read)]
    [ProducesResponseType(typeof(DormitoryDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDormitoryDetailsById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetDormitoryDetailsQuery(id);
        var dormitoryDetails = await mediator.Send(query, cancellationToken);

        return Ok(dormitoryDetails);
    }
    
    // GET: api/dormitories/{id}/rooms
    /// <summary>Получить краткую информацию о комнатах общежития.</summary>
    /// <remarks>
    /// Возвращает краткую информацию о комнатах общежития.
    /// Поддерживает пагинацию.
    /// Поддерживает фильтры по: номеру комнаты и её статусу.
    /// </remarks>
    /// <param name="id">Идентификатор комнаты.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <param name="name">Фильтр по номеру комнаты.</param>
    /// <param name="isActive">Фильтр по статусу комнаты.</param>
    /// <param name="page">Номер страницы (пагинация).</param>
    /// <param name="pageSize">Размер страницы (пагинация).</param>
    /// <response code="200">Список комнат общежития получен</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Общежитие не найдено</response>
    [HttpGet("{id:guid}/rooms")]
    [Authorize(Policy = Permissions.Rooms.Read)]
    [ProducesResponseType(typeof(PagedResult<RoomListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDormitoryRooms(
        Guid id,
        CancellationToken cancellationToken,
        [FromQuery] string? name = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = PagedQuery.DefaultPage,
        [FromQuery] int pageSize = PagedQuery.DefaultPageSize)
    {
        var query = new GetDormitoryRoomsListQuery(id, name, isActive, page, pageSize);
        var response = await mediator.Send(query, cancellationToken);

        return Ok(response);
    }
    
    
    // POST: api/dormitories
    /// <summary>Создать новое общежитие.</summary>
    /// <remarks>Возвращает идентификатор созданого общежития (GUID).</remarks>
    /// <param name="request">Параметры создаваемого общежития.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="201">Общежитие создано</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    [HttpPost]
    [Authorize(Policy = Permissions.Dormitories.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDormitoryRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateDormitoryCommand(
            request.City,
            request.Street,
            request.House,
            request.FloorCount);
        
        var dormitoryId = await mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, dormitoryId);
    }
    
    // POST: api/dormitories/{id}/add-room
    /// <summary>Добавить комнату в общежитие.</summary>
    /// <remarks>Возвращает идентификатор созданной комнаты (GUID).</remarks>
    /// <param name="id">Идентификатор общежетия в которое будет добавлена комната.</param>
    /// <param name="request">Параметры новой комнаты.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="201">Комната создана</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Общежитие не найдено</response>
    [HttpPost("{id:guid}/add-rooms")]
    [Authorize(Policy = Permissions.Rooms.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRoom(
        Guid id,
        [FromBody] CreateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRoomCommand(
            id,
            request.RoomName,
            request.Gender,
            request.Capacity,
            request.FloorNumber);

        var response = await mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }
    
    // POST /api/dormitories/{dormitory-id}/rooms/{room-id}/deactivate
    /// <summary>Сделать комнату неактивной.</summary>
    /// <param name="dormitoryId">Идентификатор общежития.</param>
    /// <param name="roomId">Идентификатор комнаты.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="200">Комната деактивированна</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    [HttpPost("{dormitory-id:guid}/rooms/{room-id}/deactivate")]
    [Authorize(Policy = Permissions.Rooms.Deactivate)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeactivateRoom(
        [FromRoute(Name = "dormitory-id")] Guid dormitoryId,
        [FromRoute(Name = "room-id")] Guid roomId,
        CancellationToken cancellationToken)
    {
        var command = new DeactivateRoomCommand(dormitoryId, roomId);
        await mediator.Send(command, cancellationToken);

        return Ok();
    }
    
    // POST /api/dormitories/{dormitory-id}/rooms/{room-id}/activate
    /// <summary>Сделать комнату активной.</summary>
    /// <param name="dormitoryId">Идентификатор общежития.</param>
    /// <param name="roomId">Идентификатор комнаты.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <response code="200">Комната активирована</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    [HttpPost("{dormitory-id:guid}/rooms/{room-id}/activate")]
    [Authorize(Policy = Permissions.Rooms.Activate)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateRoom(
        [FromRoute(Name = "dormitory-id")] Guid dormitoryId,
        [FromRoute(Name = "room-id")] Guid roomId,
        CancellationToken cancellationToken)
    {
        var command = new ActivateRoomCommand(dormitoryId, roomId);
        await mediator.Send(command, cancellationToken);

        return Ok();
    }
}