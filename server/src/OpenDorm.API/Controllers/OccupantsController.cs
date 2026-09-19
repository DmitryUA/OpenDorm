using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenDorm.API.Contracts;
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
}