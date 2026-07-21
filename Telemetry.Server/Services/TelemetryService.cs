using Microsoft.EntityFrameworkCore;
using Telemetry.Server.Data;
using Telemetry.Server.Dtos;
using Telemetry.Server.Interfaces;
using Telemetry.Server.Models;

namespace Telemetry.Server.Services
{
    public class TelemetryService : ITelemetryService
    {
        private readonly AppDbContext _context;

        public TelemetryService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<RouteResponseDto> GetData(DateTime startDate, DateTime endDate, short locomotiveId)
        {
            DateTime startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
            DateTime endUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Local).ToUniversalTime();
            
            var telemetryData = await _context.TrainLocations
                .Where(t => t.RecordedAt >= startUtc && t.RecordedAt <= endUtc && t.LocomotiveId == locomotiveId)
                .OrderBy(t => t.RecordedAt)
                .Select(t => new TrainLocationRecord
                {
                    RecordedAt = t.RecordedAt,
                    LocomotiveId = t.LocomotiveId,
                    Longitude = t.Longitude,
                    Latitude = t.Latitude,
                    Speed = t.Speed
                })
                .ToListAsync();

            var response = new RouteResponseDto { TelemetryData = telemetryData };

            if (!telemetryData.Any())
                return response;

            var firstPoint = telemetryData.First();
            var lastPoint = telemetryData.Last();
            
            var allWaypoints = await _context.Waypoints.ToListAsync();

            if (!allWaypoints.Any())
                return response;

            
            var startWaypoint = allWaypoints
                .OrderBy(w => GetDistance(firstPoint.Latitude, firstPoint.Longitude, w.Latitude, w.Longitude))
                .First();

            var endWaypoint = allWaypoints
                .OrderBy(w => GetDistance(lastPoint.Latitude, lastPoint.Longitude, w.Latitude, w.Longitude))
                .First();

            
            var minIndex = Math.Min(startWaypoint.OrderIndex, endWaypoint.OrderIndex);
            var maxIndex = Math.Max(startWaypoint.OrderIndex, endWaypoint.OrderIndex);

            var routeWaypoints = allWaypoints
                .Where(w => w.RouteId == startWaypoint.RouteId && w.OrderIndex >= minIndex && w.OrderIndex <= maxIndex)
                .OrderBy(w => w.OrderIndex)
                .ToList();
            
            response.SmoothedPath = RouteSmootherService.SmoothRoute(routeWaypoints, 10);

            return response;
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            return Math.Sqrt(Math.Pow(lat1 - lat2, 2) + Math.Pow(lon1 - lon2, 2));
        }
    }
}