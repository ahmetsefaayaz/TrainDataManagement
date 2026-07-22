using Telemetry.Simulator.Dtos;

namespace Telemetry.Simulator;

public class LocomotiveEngine
{
    public short Id { get; }
    public double CurrentLat { get; private set; }
    public double CurrentLon { get; private set; }
    public float CurrentSpeed { get; private set; }
    
    public bool IsFinished { get; private set; } = false;
    
    private readonly RouteDto _route;
    private int _currentWaypointIndex;
    private readonly List<TrainStopDto> _stops;
    private readonly Random _rnd;
    
    private int _counter = 0; 
    private int _idleCounter = 0;

    public LocomotiveEngine(short id, RouteDto route, List<TrainStopDto> stops)
    {
        Id = id;
        _route = route;
        _stops = stops;
        _rnd = new Random(id);
        _currentWaypointIndex = 0;
        
        CurrentLat = _route.Waypoints[0].Latitude + (_rnd.NextDouble() - 0.5) * 0.005;
        CurrentLon = _route.Waypoints[0].Longitude + (_rnd.NextDouble() - 0.5) * 0.005;
    }

    public void Move(double maxStepSize)
    {
        if(IsFinished) return;

        if (_idleCounter > 0)
        {
            _idleCounter--;
            CurrentSpeed = 0;
            return;
        }
        
        
        var target = _route.Waypoints[_currentWaypointIndex];
        double latDiff = target.Latitude - CurrentLat;
        double lonDiff = target.Longitude - CurrentLon;
        double distance = Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);
        
        bool isLastWaypoint = _currentWaypointIndex == _route.Waypoints.Count - 1;
        bool isIntermediateStation = _stops.Any(s => 
            Math.Abs(s.Latitude - target.Latitude) < 0.0001 && 
            Math.Abs(s.Longitude - target.Longitude) < 0.0001);
        bool isStation = isLastWaypoint || isIntermediateStation;
        
        
        double brakingDistance = maxStepSize * ((_counter * (_counter + 1)) / 2.0) / 60.0;

        if (isStation && distance <= brakingDistance)
        {
            if (_counter > 1) _counter--; 
        }
        else if (_counter < 60 && !isStation)
        {
            _counter++;
        }

        float speedRatio = _counter / 60f; 
        
        float maxSpeed = (float)(maxStepSize * 111 * 3600);

        double randomFactor = 1.0 + (_rnd.NextDouble() - 0.5) * 0.1;
        double noisySpeedRatio = speedRatio * randomFactor;
        
        CurrentSpeed = (float)(maxSpeed * noisySpeedRatio);
        
        
        double actualStepSize = maxStepSize * noisySpeedRatio;

        
        if (distance <= actualStepSize || distance < 0.000001)
        {
            CurrentLat = target.Latitude;
            CurrentLon = target.Longitude;
            
            _currentWaypointIndex++;
            if (_currentWaypointIndex >= _route.Waypoints.Count)
            {
                IsFinished = true;
                CurrentSpeed = 0;
                return;
            }

            if (isIntermediateStation)
            {
                _counter = 0;
                _idleCounter = 200; //200 saniye durakta bekle
                CurrentSpeed = 0;
                return;
            }
            
            target = _route.Waypoints[_currentWaypointIndex];
            latDiff = target.Latitude - CurrentLat;
            lonDiff = target.Longitude - CurrentLon;
            distance = Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);
        }

        if (actualStepSize > 0 && !IsFinished)
        {
            CurrentLat += (latDiff / distance) * actualStepSize; 
            CurrentLon += (lonDiff / distance) * actualStepSize;
        }
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
        string status = _counter < 60 && !IsFinished ? "[HIZLANIYOR]" : (IsFinished ? "[GARDA DURDU]" : (_counter < 60 ? "[FREN YAPIYOR]" : "[SEYİR HIZINDA]"));
        return $"[TX] [{_route.RouteName}] Loko: {Id:D2} | Lat: {CurrentLat:F4} | Lon: {CurrentLon:F4} | Hız: {CurrentSpeed:F1} km/h {status}";
    }
}