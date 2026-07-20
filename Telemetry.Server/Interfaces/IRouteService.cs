using Telemetry.Server.Dtos;

namespace Telemetry.Server.Interfaces;

public interface IRouteService
{
    Task<IEnumerable<RouteDto>> GetRoutes();
}