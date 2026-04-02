using LiquidDispenser.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace LiquidDispenser.Cloud.Data;

public class CloudDbContext : DbContext
{
    public CloudDbContext(DbContextOptions<Data.CloudDbContext> options) : base(options)
    {
    }

    public DbSet<JobRequestDto> PendingJobs { get; set; }

    public DbSet<TelemetryPayload> TelemetryLogs { get; set; }
}
