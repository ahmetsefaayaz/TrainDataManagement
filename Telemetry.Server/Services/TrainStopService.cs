using Microsoft.EntityFrameworkCore;
using Telemetry.Server.Data;
using Telemetry.Server.Dtos;
using Telemetry.Server.Interfaces;

namespace Telemetry.Server.Services;

public class TrainStopService: ITrainStopService
{
    private readonly AppDbContext _dbContext;
    public TrainStopService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IEnumerable<TrainStopDto>> GetTrainStops()
    {
        return await _dbContext.TrainStops
            .Select(t => new TrainStopDto
                { Latitude = t.Latitude, Longitude = t.Longitude, StationName = t.StationName })
            .ToListAsync();
    }
}