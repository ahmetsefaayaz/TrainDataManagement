using System.Collections.Generic;
using Telemetry.Server.Models;

namespace Telemetry.Server.Services
{
    public static class RouteSmootherService
    {
        public static List<Waypoint> SmoothRoute(List<Waypoint> waypoints, int pointsBetween = 10)
        {
            if (waypoints is null || waypoints.Count < 3)
                return waypoints;

            var smoothedWaypoints = new List<Waypoint>();

            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                var p0 = i == 0 ? waypoints[i] : waypoints[i - 1];
                var p1 = waypoints[i];
                var p2 = waypoints[i + 1];
                var p3 = i + 2 >= waypoints.Count ? waypoints[i + 1] : waypoints[i + 2];

                for (int j = 0; j <= pointsBetween; j++)
                {
                    if (j == pointsBetween && i != waypoints.Count - 2) continue;

                    double t = j / (double)pointsBetween;
                    var (lat, lon) = CatmullRom(t, p0, p1, p2, p3);

                    smoothedWaypoints.Add(new Waypoint
                    {
                        Latitude = lat,
                        Longitude = lon,
                        RouteId = p1.RouteId,
                        OrderIndex = p1.OrderIndex
                    });
                }
            }

            return smoothedWaypoints;
        }

        private static (double Lat, double Lon) CatmullRom(double t, Waypoint p0, Waypoint p1, Waypoint p2, Waypoint p3)
        {
            double t2 = t * t;
            double t3 = t2 * t;
            
            double lat = 0.5 * (
                (2.0 * p1.Latitude) +
                (-p0.Latitude + p2.Latitude) * t +
                (2.0 * p0.Latitude - 5.0 * p1.Latitude + 4.0 * p2.Latitude - p3.Latitude) * t2 +
                (-p0.Latitude + 3.0 * p1.Latitude - 3.0 * p2.Latitude + p3.Latitude) * t3
            );
            
            double lon = 0.5 * (
                (2.0 * p1.Longitude) +
                (-p0.Longitude + p2.Longitude) * t +
                (2.0 * p0.Longitude - 5.0 * p1.Longitude + 4.0 * p2.Longitude - p3.Longitude) * t2 +
                (-p0.Longitude + 3.0 * p1.Longitude - 3.0 * p2.Longitude + p3.Longitude) * t3
            );

            return (lat, lon);
        }
    }
}