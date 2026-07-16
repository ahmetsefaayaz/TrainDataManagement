using Telemetry.Server.Models;

namespace Telemetry.Server.Interfaces;

public interface ITelemetryService
{
    Task<List<TrainLocationRecord>> GetData(DateTime startDate, DateTime endDate, short locomotiveId);
}