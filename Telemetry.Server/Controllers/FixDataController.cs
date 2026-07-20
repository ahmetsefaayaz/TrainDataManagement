using Microsoft.AspNetCore.Mvc;
using Telemetry.Server.Interfaces;

namespace Telemetry.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FixDataController: ControllerBase
{
    private readonly IFixDataService _service;
    public FixDataController(IFixDataService service)
    {
        _service = service;
    }

    [HttpGet("closest")]
    public async Task<IActionResult> GetClosestWaypoints(double startLat, double startLon, double endLat, double endLon, int routeId)
    {
        var waypoints = await _service.GetClosestWaypoints(startLat, startLon, endLat, endLon, routeId);
        return Ok(waypoints);
    }

    [HttpGet("fix")]
    public async Task<IActionResult> Fix(double startLat, double startLon, double endLat, double endLon, int routeId)
    {
        var closest = await _service.GetClosestWaypoints(startLat, startLon, endLat, endLon, routeId);
        
        var result = await _service.FixWaypoints(closest); 
        
        return Ok(result);
    }
    
}