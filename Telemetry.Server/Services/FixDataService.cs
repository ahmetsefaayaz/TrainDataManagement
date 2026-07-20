using Microsoft.EntityFrameworkCore;
using Telemetry.Server.Data;
using Telemetry.Server.Interfaces;
using Telemetry.Server.Models;

namespace Telemetry.Server.Services;

public class FixDataService: IFixDataService
{
    private readonly AppDbContext _context;

    public FixDataService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Waypoint>> GetClosestWaypoints(double startLat, double startLon, double endLat, double endLon, int routeId)
    {
        var waypointsInRoute = await _context.Waypoints
            .Where(w => w.RouteId == routeId)
            .ToListAsync();

        if (!waypointsInRoute.Any()) return new List<Waypoint>();

        Waypoint waypointClosestToStart = waypointsInRoute.First();
        Waypoint waypointClosestToEnd = waypointsInRoute.First();
    
        double distanceStart = GetDistance(startLat, startLon, waypointClosestToStart);
        double distanceEnd = GetDistance(endLat, endLon, waypointClosestToEnd);
    
        foreach (var waypoint in waypointsInRoute)
        {
            double currentCheckStart = GetDistance(startLat, startLon, waypoint);
            if (currentCheckStart < distanceStart)
            {
                distanceStart = currentCheckStart;
                waypointClosestToStart = waypoint;
            }

            double currentCheckEnd = GetDistance(endLat, endLon, waypoint);
            if(currentCheckEnd < distanceEnd)
            {
                distanceEnd = currentCheckEnd;
                waypointClosestToEnd = waypoint;
            }
        }
        return new List<Waypoint>
        {
            waypointClosestToStart,
            waypointClosestToEnd
        };
    }

    public async Task<List<Waypoint>> FixWaypoints(List<Waypoint> waypoints)
    {
        if (waypoints == null || waypoints.Count < 2)
        {
            return new List<Waypoint>();
        }
        var prevWaypoint = waypoints[0];
        var currentWaypoint = waypoints[1];
        
        var minIndex = Math.Min(prevWaypoint.OrderIndex, currentWaypoint.OrderIndex);
        var maxIndex = Math.Max(prevWaypoint.OrderIndex, currentWaypoint.OrderIndex);
        
        var routeId = prevWaypoint.RouteId;
        
        var waypointsToFix = await _context.Waypoints
            .Where(w => w.OrderIndex >= minIndex && w.OrderIndex <= maxIndex && w.RouteId == routeId)
            .OrderBy(w => w.OrderIndex)
            .ToListAsync();
        return waypointsToFix;
    }

    public double GetDistance(double lat, double lon, Waypoint waypoint)
    {
        var x = Math.Abs(lat - waypoint.Latitude);
        var y = Math.Abs(lon - waypoint.Longitude);
        return Math.Sqrt(x * x + y * y);
    }
    
}