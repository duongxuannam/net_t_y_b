using Microsoft.AspNetCore.Mvc;

namespace NetTYB.Api.Controllers;

[ApiController]
[Route("api")]
public class OpsController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "ok", message = "Service is healthy." });
    }

    [HttpGet("metrics")]
    public IActionResult Metrics()
    {
        return Content("net_t_y_b_requests_total 1\n", "text/plain");
    }

    [HttpGet("status")]
    public IActionResult Status()
    {
        return Ok(new { name = "Net T Y B", status = "ok" });
    }
}
