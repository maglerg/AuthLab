
using AuthLab.Provider.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthLab.Provider.Controllers;

[ApiController]
[Route(".well-known")]
public class JwksController : ControllerBase
{
    private readonly JwksService _jwksService;

    public JwksController(JwksService jwksService)
    {
        _jwksService = jwksService;
    }

    [AllowAnonymous]
    [HttpGet("jwks.json")]
    public IActionResult GetJwks()
    {
        var jwks = _jwksService.GetJwks();

        return Ok(jwks);
    }
}
