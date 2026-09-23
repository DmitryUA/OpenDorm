using MediatR;
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
using Swashbuckle.AspNetCore.Annotations;

namespace OpenDorm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DormitoriesController(IMediator mediator) : ControllerBase
{
    // GET: api/dormitories
    [HttpGet]
    [SwaggerOperation(
        Summary = "Получить список всех общежитий.",
        Description = "Возвращает краткую информацию по каждому общежитию.")]
    [ProducesResponseType(typeof(IReadOnlyCollection<DormitoryListDto>),StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetDormitoryListQuery();
        var response = await mediator.Send(query, cancellationToken);

        return Ok(response);
    }
    
    // GET: api/dormitories/{id}/details
    [HttpGet("{id:guid}/details")]
    [SwaggerOperation(
        Summary = "Получить детальную информацию об общежитии по идентификатору.",
        Description = "Вовзращает детальную информацию об общежитии по его идентификатору.")]
    [ProducesResponseType(typeof(DormitoryDetailsDto), StatusCodes.Status200OK)]
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
    [HttpGet("{id:guid}/rooms")]
    [SwaggerOperation(
        Summary = "Получить краткую информацию о комнатах общежития",
        Description = "Возвращает краткую информацию о комнатах общежития." +
                      "\nПоддерживает пагинацию." +
                      "\nПоддерживает фильтры по: номеру комнаты и её статусу.")]
    [ProducesResponseType(typeof(PagedResult<RoomListDto>), StatusCodes.Status200OK)]
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
    [HttpPost]
    [SwaggerOperation(Summary = "Создать новое общежитие.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    
    // POST: api/dormitories/{dormitory-id}/rooms
    [HttpPost("{dormitory-id:guid}/rooms")]
    [SwaggerOperation(
        Summary = "Добавить комнату в общежитие.",
        Description = "Возвращает идентификатор созданной комнаты.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateRoom(
        [FromRoute(Name = "dormitory-id")] Guid dormitoryId,
        [FromBody] CreateRoomRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRoomCommand(
            dormitoryId,
            request.RoomName,
            request.Gender,
            request.Capacity,
            request.FloorNumber);

        var response = await mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }
    
    // POST /api/dormitories/{dormitoryId}/rooms/{roomId}/deactivate
    [HttpPost("{dormitory-id:guid}/rooms/{room-id}/deactivate")]
    [SwaggerOperation(Summary = "Сделать комнату неактивной")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
    
    // POST /api/dormitories/{dormitoryId}/rooms/{roomId}/activate
    [HttpPost("{dormitory-id:guid}/rooms/{room-id}/activate")]
    [SwaggerOperation(Summary = "Сделать комнату активной.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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