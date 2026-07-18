using Microsoft.EntityFrameworkCore;
using Telemetry.Server.Data;
using Telemetry.Server.Interfaces;
using Telemetry.Server.Models;

namespace Telemetry.Server.Services;

public class TelemetryService: ITelemetryService
{
    private readonly AppDbContext _context;

    public TelemetryService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<List<TrainLocationRecord>> GetData(DateTime startDate, DateTime endDate, short locomotiveId)
    {
        DateTime startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
        DateTime endUtc = DateTime.SpecifyKind(endDate, DateTimeKind.Local).ToUniversalTime();

        var query = _context.TrainLocations
            .Where(t => t.RecordedAt >= startUtc && t.RecordedAt <= endUtc && t.LocomotiveId == locomotiveId);
            
        var routeData = await query
            .OrderBy(t => t.RecordedAt)
            .Select(t => new TrainLocationRecord
            {
                RecordedAt = t.RecordedAt,
                LocomotiveId = t.LocomotiveId,
                Longitude =  t.Longitude,
                Latitude = t.Latitude,
                Speed = t.Speed
            })
            .ToListAsync();
            
        return routeData;
    }
}