using Telemetry.Server.Dtos;

namespace Telemetry.Server.Interfaces;

public interface ITrainStopService
{
    Task <IEnumerable<TrainStopDto>>  GetTrainStops();
}