using Microsoft.EntityFrameworkCore;
using Telemetry.Server.Models;

namespace Telemetry.Server.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
    public DbSet<TrainLocationRecord> TrainLocations { get; set; }
    
}