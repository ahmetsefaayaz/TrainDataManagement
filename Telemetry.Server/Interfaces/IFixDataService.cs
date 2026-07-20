using Telemetry.Server.Models;

namespace Telemetry.Server.Interfaces;

public interface IFixDataService
{
    Task<List<Waypoint>> GetClosestWaypoints(double startLat, double startLon, double endLat, double endLon, int routeId);
    Task<List<Waypoint>> FixWaypoints(List<Waypoint> waypoints);
}