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

    [HttpGet("simulation")]
    public async Task<IActionResult> GetSimulation(short locomotiveId, DateTime startDate, DateTime endDate)
    {
        var startUtc = startDate.ToUniversalTime();
        var endUtc = endDate.ToUniversalTime();
        var simulationFrames = await _telemetryService.GetSimulationFramesAsync(locomotiveId, startUtc,endUtc);
    
        if (simulationFrames == null || !simulationFrames.Any())
        {
            return NotFound("Bu tarih aralığında simülasyon verisi bulunamadı.");
        }

        return Ok(simulationFrames);
    }
}