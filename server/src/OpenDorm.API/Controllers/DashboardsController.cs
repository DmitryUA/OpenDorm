using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDorm.Application.Features.Dashboards.Queries.GetSummary;

namespace OpenDorm.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardsController(IMediator mediator) : ControllerBase
{
    /// <summary>Получить общую статистику по количеству общежитий, комнат, жильцов, заселений.</summary>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Статистика получена.</response>
    /// <response code="401">Пользователь не аутентифицирован.</response>
    /// <response code="403">Недостаточно прав.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(SummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSummary(
        CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetSummaryQuery(), cancellationToken);

        return Ok(response);
    }
}