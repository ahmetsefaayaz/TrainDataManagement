namespace Telemetry.Server.Models;

public class TrainLocationRecord
{
    public int Id { get; set; }
    public short LocomotiveId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public float Speed { get; set; }
    public DateTime RecordedAt { get; set; }
}