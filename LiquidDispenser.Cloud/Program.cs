using LiquidDispenser.Cloud.Components;
using Microsoft.EntityFrameworkCore;
using LiquidDispenser.Core;
using LiquidDispenser.Core.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Persistence ---
builder.Services.AddDbContext<LiquidDispenser.Cloud.Data.CloudDbContext>(options =>
    options.UseSqlite("Data Source=dispenser.db"));

// --- Blazor ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOpenApi();

var app = builder.Build();

// Ensure the database schema is created, seed demo jobs if empty
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LiquidDispenser.Cloud.Data.CloudDbContext>();
    db.Database.EnsureCreated();

    if (!db.PendingJobs.Any())
    {
        db.PendingJobs.Add(new JobRequestDto { SourceRowStart = 0, SourceColumnIndex = 0, DestRowStart = 0, DestColumn = 0, Volume = 10.0 });
        db.PendingJobs.Add(new JobRequestDto { SourceRowStart = 8, SourceColumnIndex = 0, DestRowStart = 8, DestColumn = 0, Volume = 10.0 });
        db.PendingJobs.Add(new JobRequestDto { SourceRowStart = 0, SourceColumnIndex = 1, DestRowStart = 16, DestColumn = 0, Volume = 10.0 });
        db.PendingJobs.Add(new JobRequestDto { SourceRowStart = 8, SourceColumnIndex = 1, DestRowStart = 0, DestColumn = 1, Volume = 10.0 });
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseStaticFiles();
app.UseAntiforgery();

// --- Minimal API Endpoints ---

app.MapGet("/api/jobs/pending", async (LiquidDispenser.Cloud.Data.CloudDbContext db) =>
{
    var jobs = await db.PendingJobs.Take(5).ToListAsync();
    if (jobs.Any())
    {
        db.PendingJobs.RemoveRange(jobs);
        await db.SaveChangesAsync();
    }
    return Results.Ok(jobs);
});

app.MapPost("/api/jobs/add", async (JobRequestDto job, LiquidDispenser.Cloud.Data.CloudDbContext db) =>
{
    db.PendingJobs.Add(job);
    await db.SaveChangesAsync();
    return Results.Ok(new { Status = "Added", JobId = job.JobId });
});

app.MapPost("/api/telemetry", async (TelemetryPayload data, LiquidDispenser.Cloud.Data.CloudDbContext db) =>
{
    db.TelemetryLogs.Add(data);
    await db.SaveChangesAsync();
    return Results.Ok(new { Status = "Received" });
});

app.MapGet("/api/telemetry/latest", async (LiquidDispenser.Cloud.Data.CloudDbContext db) =>
{
    var latest = await db.TelemetryLogs.OrderByDescending(t => t.Id).FirstOrDefaultAsync();
    return Results.Ok(latest);
});

app.MapGet("/api/telemetry/history", async (LiquidDispenser.Cloud.Data.CloudDbContext db) =>
{
    var history = await db.TelemetryLogs.OrderByDescending(t => t.Id).Take(50).ToListAsync();
    return Results.Ok(history);
});

// --- Blazor ---
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
