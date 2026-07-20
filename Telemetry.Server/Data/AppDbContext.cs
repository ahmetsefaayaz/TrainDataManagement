using Microsoft.EntityFrameworkCore;
using Telemetry.Server.Models;
using Route = Telemetry.Server.Models.Route;

namespace Telemetry.Server.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public DbSet<TrainLocationRecord> TrainLocations { get; set; }
    public DbSet<Waypoint> Waypoints { get; set; }
    public DbSet<TrainStop> TrainStops { get; set; }
    public DbSet<Route> Routes { get; set; }
    
}