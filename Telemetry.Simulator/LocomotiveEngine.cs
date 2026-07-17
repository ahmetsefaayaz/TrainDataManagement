
namespace Telemetry.Simulator;

public class LocomotiveEngine
{
    public short Id { get; }
    public double CurrentLat { get; private set; }
    public double CurrentLon { get; private set; }
    public float CurrentSpeed { get; private set; }
    
    public bool IsFinished { get; private set; } = false;
    
    private readonly RailwayRoute _route;
    private int _currentWaypointIndex;
    private readonly Random _rnd;

    public LocomotiveEngine(short id, RailwayRoute route)
    {
        Id = id;
        _route = route;
        _rnd = new Random(id);
        _currentWaypointIndex = 0;
        
        CurrentLat = _route.Waypoints[0].Lat + (_rnd.NextDouble() - 0.5) * 0.005;
        CurrentLon = _route.Waypoints[0].Lon + (_rnd.NextDouble() - 0.5) * 0.005;
    }

    public void Move(double stepSize)
    {
        if(IsFinished) return;
        
        var target = _route.Waypoints[_currentWaypointIndex];
        double latDiff = target.Lat - CurrentLat;
        double lonDiff = target.Lon - CurrentLon;
        double distance = Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);

        if (distance < 0.001)
        {
            _currentWaypointIndex++;
            if (_currentWaypointIndex >= _route.Waypoints.Count)
            {
                IsFinished = true;
                CurrentSpeed = 0;
                return;
            }
            target = _route.Waypoints[_currentWaypointIndex];
            latDiff = target.Lat - CurrentLat;
            lonDiff = target.Lon - CurrentLon;
            distance = Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);
        }
        
        if (_route.RouteName.Contains("YHT"))
            CurrentSpeed = _rnd.Next(200, 250);
        else
            CurrentSpeed = _rnd.Next(60, 110);
        
        
        CurrentLat += (latDiff / distance) * stepSize;
        CurrentLon += (lonDiff / distance) * stepSize;
    }

    public byte[] GetPayload()
    {
        byte[] idBytes = BitConverter.GetBytes(Id);
        byte[] latBytes = BitConverter.GetBytes(CurrentLat);
        byte[] lonBytes = BitConverter.GetBytes(CurrentLon);
        byte[] speedBytes = BitConverter.GetBytes(CurrentSpeed);

        byte[] payload = new byte[22];
        Buffer.BlockCopy(idBytes, 0, payload, 0, 2);
        Buffer.BlockCopy(latBytes, 0, payload, 2, 8);
        Buffer.BlockCopy(lonBytes, 0, payload, 10, 8);
        Buffer.BlockCopy(speedBytes, 0, payload, 18, 4);

        return payload;
    }

    public string GetStatusLog()
    {
        return $"[TX] [{_route.RouteName}] Loko: {Id:D2} | Lat: {CurrentLat:F4} | Lon: {CurrentLon:F4} | Hız: {CurrentSpeed} km/h";
    }
}