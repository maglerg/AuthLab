using AuthLab.Provider.DTOs;
using AuthLab.Provider.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;

namespace AuthLab.Provider.Controllers;

[ApiController]
[Route("connect")]
public class AuthorizationController : ControllerBase
{
    private readonly AuthorizationService _authorizationService;

    public AuthorizationController(
        AuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    [Authorize]
    [HttpGet("authorize")]
    public async Task<IActionResult> Authorize(
        [FromQuery(Name = "response_type")] string responseType,
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri,
        [FromQuery(Name = "state")] string state,
        [FromQuery(Name = "code_challenge")] string codeChallenge,
        [FromQuery(Name = "code_challenge_method")]
        string codeChallengeMethod)
    {
        if (responseType != "code" ||
            string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(redirectUri) ||
            string.IsNullOrWhiteSpace(state) ||
            string.IsNullOrWhiteSpace(codeChallenge) ||
            codeChallengeMethod != "S256")
        {
            return BadRequest(new
            {
                error = "Invalid authorization request."
            });
        }

        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var code = await _authorizationService.CreateCodeAsync(
            userId,
            clientId,
            redirectUri,
            codeChallenge);

        if (code is null)
        {
            return BadRequest(new
            {
                error = "Invalid client or redirect URI."
            });
        }

        var callbackUrl = QueryHelpers.AddQueryString(
            redirectUri,
            new Dictionary<string, string?>
            {
                ["code"] = code,
                ["state"] = state
            });

        return Redirect(callbackUrl);
    }
    [AllowAnonymous]
    [HttpGet("debug-auth")]
    public IActionResult DebugAuth()
    {
        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated,
            AuthenticationType = User.Identity?.AuthenticationType,
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            HasCookie = Request.Cookies.Count > 0
        });
    }
    [AllowAnonymous]
    [HttpPost("token")]
    public async Task<IActionResult> Token(
        [FromForm] TokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code) ||
            string.IsNullOrWhiteSpace(request.ClientId) ||
            string.IsNullOrWhiteSpace(request.RedirectUri) ||
            string.IsNullOrWhiteSpace(request.CodeVerifier))
        {
            return BadRequest(new
            {
                error = "invalid_request"
            });
        }

        var response =
            await _authorizationService.ExchangeCodeAsync(request);

        if (response is null)
        {
            return BadRequest(new
            {
                error = "invalid_grant"
            });
        }

        return Ok(response);
    }
}