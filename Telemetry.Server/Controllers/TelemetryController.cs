using Microsoft.AspNetCore.Mvc;
using Telemetry.Server.Interfaces;
using Telemetry.Server.Services;

namespace Telemetry.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelemetryController : ControllerBase
{
    private readonly ITelemetryService _telemetryService;

    public TelemetryController(ITelemetryService telemetryService)
    {
        _telemetryService = telemetryService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetData(DateTime startDate, DateTime endDate, short locomotiveId)
    {
        var result = await _telemetryService.GetData(startDate, endDate, locomotiveId);
        return Ok(result);
    }
}