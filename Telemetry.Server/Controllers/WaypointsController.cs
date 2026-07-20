using Microsoft.AspNetCore.Mvc;
using Telemetry.Server.Interfaces;

namespace Telemetry.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WaypointsController: ControllerBase
{
    private readonly IWaypointService _service;

    public WaypointsController(IWaypointService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetWaypoints()
    {
        var result = await _service.GetWaypoints();
        return Ok(result);
    }
}