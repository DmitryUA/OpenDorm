using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenDorm.Application.Features.Auth.Commands;
using OpenDorm.Application.Features.Auth.Commands.Login;
using OpenDorm.Application.Features.Auth.Commands.Logout;
using OpenDorm.Application.Features.Auth.Commands.RefreshToken;

namespace OpenDorm.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ISender sender) : ControllerBase
{
    /// <summary>Аутентификация пользователя.</summary>
    /// <remarks>Возвращает пару токенов: access (JWT) и refresh.</remarks>
    /// <response code="200">Успешный вход. Возвращает токены.</response>
    /// <response code="400">Неверный логин или пароль, либо пользователь деактивирован.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] Contracts.LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Login, request.Password);
        var result = await sender.Send(command, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>Обновление access token по refresh token.</summary>
    /// <remarks>Старый refresh token отзывается (ротация), выдаётся новая пара токенов.</remarks>
    /// <response code="200">Токены успешно обновлены.</response>
    /// <response code="400">Невалидный, истёкший или отозванный refresh token.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Refresh(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>Выход из системы (отзыв refresh token).</summary>
    /// <remarks>Access token продолжает действовать до истечения, но refresh token больше нельзя использовать.</remarks>
    /// <response code="204">Успешный выход.</response>
    /// <response code="400">Refresh token не найден.</response>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutCommand command,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        
        return NoContent();
    }
}