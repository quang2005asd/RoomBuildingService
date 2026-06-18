using Microsoft.EntityFrameworkCore;
using RoomBuildingService.Api;
using RoomBuildingService.Api.Middlewares;
using RoomBuildingService.Core.Interfaces;
using RoomBuildingService.Infrastructure.BackgroundWorkers;
using RoomBuildingService.Infrastructure.Messaging;
using RoomBuildingService.Infrastructure.Persistence;
using RoomBuildingService.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");
var connectionString =
    builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (!useInMemoryDatabase && string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Database connection string is not configured.");
}

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    if (useInMemoryDatabase)
    {
        opt.UseInMemoryDatabase("RoomBuildingServiceDev");
        return;
    }

    opt.UseNpgsql(
        connectionString,
        sql => sql.MigrationsAssembly("RoomBuildingService.Infrastructure"));
});

builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IBedRepository, BedRepository>();
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();

builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddHostedService<OutboxWorker>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt =>
    opt.AddPolicy("AllowAll", p =>
        p.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (db.Database.IsRelational())
    {
        db.Database.EnsureCreated();
    }

    SeedData.Initialize(db);
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Room & Building Service API v1");
    c.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"));
app.UseExceptionHandler(_ => { });
app.UseCors("AllowAll");
app.MapControllers();
app.Run();

public partial class Program { }
