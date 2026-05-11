using Microsoft.AspNetCore.Mvc;

namespace CasaMulher.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "API da Casa da Mulher funcionando",
            data = DateTime.UtcNow
        });
    }
}
