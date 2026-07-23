namespace Telemetry.Server.Dtos;

public class SimulationFrameDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public float Speed { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsActive { get; set; }
}