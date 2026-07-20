using Microsoft.AspNetCore.Mvc;
using Telemetry.Server.Interfaces;

namespace Telemetry.Server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class TrainStopsController: ControllerBase
{
    private readonly ITrainStopService _service;
    public TrainStopsController(ITrainStopService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetTrainStops()
    {
        var result = await _service.GetTrainStops();
        return Ok(result);
    }
}