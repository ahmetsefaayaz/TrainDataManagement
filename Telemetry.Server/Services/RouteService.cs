using Microsoft.EntityFrameworkCore;
using Telemetry.Server.Data;
using Telemetry.Server.Dtos;
using Telemetry.Server.Interfaces;

namespace Telemetry.Server.Services;

public class RouteService: IRouteService
{
    private readonly AppDbContext _dbContext;
    public RouteService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    
    public async Task<IEnumerable<RouteDto>> GetRoutes()
    {
        return await _dbContext.Routes
            .Include(x => x.Waypoints)
            .Include(x => x.TrainStops)
            .Select(r => new RouteDto
            {
                Id = r.Id,
                RouteName = r.RouteName,
                Waypoints = r.Waypoints.OrderBy(w => w.OrderIndex)
                    .Select(w => new WaypointDto
                        { Latitude = w.Latitude, Longitude = w.Longitude, OrderIndex = w.OrderIndex })
                    .ToList(),
                TrainStops = r.TrainStops.Select(t => new TrainStopDto{
                    Latitude = t.Latitude,
                    Longitude = t.Longitude,
                    StationName = t.StationName}).ToList()
            }).ToListAsync();
    }
}