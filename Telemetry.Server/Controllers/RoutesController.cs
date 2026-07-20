using Microsoft.AspNetCore.Mvc;
using Telemetry.Server.Dtos;
using Telemetry.Server.Interfaces;

namespace Telemetry.Server.Controllers;


[ApiController]
[Route("api/[controller]")]
public class RoutesController: ControllerBase
{
    private readonly IRouteService _service;

    public RoutesController(IRouteService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetRoutes()
    {
        var result = await _service.GetRoutes();
        return Ok(result);
    }
    
}