using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDorm.API.Contracts;
using OpenDorm.Application.Common;
using OpenDorm.Application.Features.Users;
using OpenDorm.Application.Features.Users.Commands.AssignRole;
using OpenDorm.Application.Features.Users.Commands.CreateUser;
using OpenDorm.Application.Features.Users.Commands.DeactivateUser;
using OpenDorm.Application.Features.Users.Commands.RevokeRole;
using OpenDorm.Application.Features.Users.Commands.UpdateUser;
using OpenDorm.Application.Features.Users.Queries.Common;
using OpenDorm.Application.Features.Users.Queries.GetUserById;
using OpenDorm.Application.Features.Users.Queries.GetUserList;

namespace OpenDorm.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(ISender sender) : ControllerBase
{
    // GET: api/users
    /// <summary>Получить список всех пользователей.</summary>
    /// <response code="200">Список пользователей успешно получен</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    [HttpGet]
    [Authorize(Policy = Permissions.Users.Read)]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetUserListQuery();
        var result = await sender.Send(query, cancellationToken);
        
        return Ok(result);
    }

    // GET: api/users/{id}
    /// <summary>Получить пользователя по идентификатору.</summary>
    /// <response code="200">Пользователь найден</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Пользователь не найден</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.Users.Read)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);
        
        return Ok(result);
    }

    // POST: api/users
    /// <summary>Создать нового пользователя.</summary>
    /// <response code="201">Пользователь успешно создан</response>
    /// <response code="400">Невалидные данные или логин уже занят</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    [HttpPost]
    [Authorize(Policy = Permissions.Users.Create)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(
            request.Login,
            request.Password,
            request.LastName,
            request.FirstName,
            request.Patronymic,
            request.RoleName);
        
        var id = await sender.Send(command, cancellationToken);
        
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    // PUT: api/users/{id}
    /// <summary>Обновить данные пользователя.</summary>
    /// <response code="204">Пользователь успешно обновлён</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Пользователь не найден</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Users.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(
            id,
            request.LastName,
            request.FirstName,
            request.Patronymic,
            request.NewPassword);
        
        await sender.Send(command, cancellationToken);
        
        return NoContent();
    }

    // POST: api/users/{id}/add-role
    /// <summary>Назначить роль пользователю.</summary>
    /// <response code="204">Роль успешно назначена</response>
    /// <response code="400">У пользователя уже есть эта роль</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Пользователь или роль не найдены</response>
    [HttpPost("{id:guid}/assign-role")]
    [Authorize(Policy = Permissions.Users.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole(
        Guid id,
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignRoleCommand(id, request.RoleName);
        await sender.Send(command, cancellationToken);
        
        return NoContent();
    }

    // DELETE: api/users/{id}/revoke-role
    /// <summary>Отозвать роль у пользователя.</summary>
    /// <response code="204">Роль успешно отозвана</response>
    /// <response code="400">У пользователя нет этой роли</response>
    /// <response code="404">Пользователь или роль не найдены</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    [HttpDelete("{id:guid}/revoke-role")]
    [Authorize(Policy = Permissions.Users.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeRole(
        Guid id,
        [FromBody] RevokeRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RevokeRoleCommand(id, request.RoleName);
        await sender.Send(command with { UserId = id }, cancellationToken);
        
        return NoContent();
    }

    // POST: api/users/{id}
    /// <summary>Деактивировать пользователя.</summary>
    /// <response code="204">Пользователь успешно деактивирован</response>
    /// <response code="401">Пользователь не аутентифицирован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="404">Пользователь не найден</response>
    [HttpPost("{id:guid}")]
    [Authorize(Policy = Permissions.Users.Deactivate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeactivateUserCommand(id);
        await sender.Send(command, cancellationToken);
        
        return NoContent();
    }
}