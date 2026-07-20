using Microsoft.EntityFrameworkCore;
using Telemetry.Server.Data;
using Telemetry.Server.Dtos;
using Telemetry.Server.Interfaces;

namespace Telemetry.Server.Services;

public class WaypointService: IWaypointService
{
    private readonly AppDbContext _context;
    public WaypointService(AppDbContext context)
    {
        _context = context;
    }


    public async Task<IEnumerable<WaypointDto>> GetWaypoints()
    {
        return await _context.Waypoints
            .Select(w => new WaypointDto { Latitude = w.Latitude, Longitude = w.Longitude, OrderIndex = w.OrderIndex })
            .ToListAsync();
    }
}