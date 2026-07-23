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

            if (!telemetryData.Any()) return response;
            response = await CreateSegmentsAsync(response);
            return response;
        }
        private List<Waypoint> GetSmoothedWaypointsBetween(TrainLocationRecord start, TrainLocationRecord end, List<Waypoint> allWaypoints)
        {
            var startWp = allWaypoints.OrderBy(w => GetDistance(start.Latitude, start.Longitude, w.Latitude, w.Longitude)).First();
            var endWp = allWaypoints.OrderBy(w => GetDistance(end.Latitude, end.Longitude, w.Latitude, w.Longitude)).First();

            var minIndex = Math.Min(startWp.OrderIndex, endWp.OrderIndex);
            var maxIndex = Math.Max(startWp.OrderIndex, endWp.OrderIndex);

            var routeWaypoints = allWaypoints
                .Where(w => w.RouteId == startWp.RouteId && w.OrderIndex >= minIndex && w.OrderIndex <= maxIndex)
                .OrderBy(w => w.OrderIndex)
                .ToList();

            return RouteSmootherService.SmoothRoute(routeWaypoints, 10);
        }
        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            return Math.Sqrt(Math.Pow(lat1 - lat2, 2) + Math.Pow(lon1 - lon2, 2));
        }
        public async Task<List<SimulationFrameDto>> GetSimulationFramesAsync(short trainId, DateTime startTime, DateTime endTime)
        {
            
            var rawRecords = await _context.TrainLocations
                .Where(r => r.LocomotiveId == trainId && r.RecordedAt >= startTime && r.RecordedAt <= endTime)
                .OrderBy(r => r.RecordedAt)
                .ToListAsync();
            
            var frames = await SimulationMapMatching(rawRecords);

            return frames;
        }
        public async Task<RouteResponseDto> CreateSegmentsAsync(RouteResponseDto response)
        {
            var currentRawSegment = new List<TrainLocationRecord>();
            var allWaypoints = await _context.Waypoints.ToListAsync();
            if (!allWaypoints.Any()) return response;
            for (int i = 0; i < response.TelemetryData.Count; i++)
            {
                var point = response.TelemetryData[i];

                if (i > 0)
                {
                    var prevPoint = response.TelemetryData[i - 1];
                    var timeDiff = (point.RecordedAt - prevPoint.RecordedAt).TotalMilliseconds;

                    if (timeDiff > 2000)
                    {
                        if (currentRawSegment.Any())
                        {
                            response.Segments.Add(new RouteSegmentDto
                            {
                                IsGap = false,
                                Coordinates = GetSmoothedWaypointsBetween(currentRawSegment.First(), currentRawSegment.Last(), allWaypoints)
                            });
                            currentRawSegment.Clear();
                        }

                        response.Segments.Add(new RouteSegmentDto
                        {
                            IsGap = true,
                            Coordinates = GetSmoothedWaypointsBetween(prevPoint, point, allWaypoints)
                        });
                    }
                }

                currentRawSegment.Add(point);
            }
            if (currentRawSegment.Any())
            {
                response.Segments.Add(new RouteSegmentDto
                {
                    IsGap = false,
                    Coordinates = GetSmoothedWaypointsBetween(currentRawSegment.First(), currentRawSegment.Last(), allWaypoints)
                });
            }
            return response;
        }
        public async Task<List<SimulationFrameDto>> SimulationMapMatching(List<TrainLocationRecord> records)
        {
            var allWaypoints = await _context.Waypoints.ToListAsync();
            var frames = new List<SimulationFrameDto>();
            for (int i = 0; i < records.Count; i++)
            {
                var current = records[i];
                bool canMapMatch = allWaypoints.Any(w => 
                    GetDistance(w.Latitude, w.Longitude, current.Latitude, current.Longitude) < 0.002);
                bool isActive = true;
                if (i > 0)
                {
                    var previous = records[i - 1];
                    if ((current.RecordedAt - previous.RecordedAt).TotalMilliseconds > 5000)
                    {
                        isActive = false;
                    }
                }
                var matchedLat = current.Latitude;
                var matchedLon = current.Longitude;
                
                if (canMapMatch)
                {
                    var closestWaypoint = allWaypoints
                        .OrderBy(w => GetDistance(current.Latitude, current.Longitude, w.Latitude, w.Longitude))
                        .First();
                
                    matchedLat = closestWaypoint.Latitude;
                    matchedLon = closestWaypoint.Longitude;
                }

                frames.Add(new SimulationFrameDto
                {
                    Latitude = matchedLat,
                    Longitude = matchedLon,
                    Speed = current.Speed,
                    Timestamp = current.RecordedAt,
                    IsActive = isActive
                });
            }
            return frames;
        }
    }
    
}