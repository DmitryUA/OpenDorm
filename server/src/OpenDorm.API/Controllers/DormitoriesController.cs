using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenDorm.Application.Features.Dormitories.Commands.CreateDormitory;
using OpenDorm.Application.Features.Dormitories.Queries.GetDormitoryList;

namespace OpenDorm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DormitoriesController(IMediator mediator) : ControllerBase
{
    // GET: api/dormitories
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetDormitoryListQuery();

        var dormitories = await mediator.Send(query, cancellationToken);

        return Ok(dormitories);
    }
    
    // POST: api/dormitories
    [HttpPost]
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