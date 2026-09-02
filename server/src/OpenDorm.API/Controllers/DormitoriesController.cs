using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenDorm.Application.Features.Dormitories.Commands.CreateDormitory;
using OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryDetailsById;
using OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryList;
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

        var dormitories = await mediator.Send(query, cancellationToken);

        return Ok(dormitories);
    }
    
    // GET: api/dormitories
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Получить детальную информацию об общежитии по идентификатору.",
        Description = "Вовзращает детальную информацию об общежитии по его идентификатору.")]
    [ProducesResponseType(typeof(DormitoryDetailsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDormitoryDetailsById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetDormitoryDetailsByIdQuery(id);
        var dormitoryDetails = await mediator.Send(query, cancellationToken);

        return Ok(dormitoryDetails);
    }
    
    // POST: api/dormitories
    [HttpPost]
    [SwaggerOperation(Summary = "Создать новое общежитие.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDormitoryCommand command,
        CancellationToken cancellationToken)
    {
        var dormitoryId = await mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, dormitoryId);
    }
}