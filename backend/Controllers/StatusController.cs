using Microsoft.AspNetCore.Mvc;

namespace NetTYB.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            name = "Net T Y B",
            status = "ok",
            message = "ASP.NET Core backend ready for the React frontend."
        });
    }
}
