using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using RoomBuildingService.Api;
using RoomBuildingService.Api.Middlewares;
using RoomBuildingService.Core.Interfaces;
using RoomBuildingService.Infrastructure.BackgroundWorkers;
using RoomBuildingService.Infrastructure.Messaging;
using RoomBuildingService.Infrastructure.Persistence;
using RoomBuildingService.Infrastructure.Persistence.Repositories;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");
var rawConnectionString =
    builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (!useInMemoryDatabase && string.IsNullOrWhiteSpace(rawConnectionString))
{
    throw new InvalidOperationException("Database connection string is not configured.");
}

var connectionString = NormalizeConnectionString(rawConnectionString);

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
        EnsureBedIntegrationColumns(db.Database);
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

static void EnsureBedIntegrationColumns(DatabaseFacade database)
{
    database.ExecuteSqlRaw("""
        ALTER TABLE public."Beds" ADD COLUMN IF NOT EXISTS "StudentId" uuid NULL;
        ALTER TABLE public."Beds" ADD COLUMN IF NOT EXISTS "StudentName" character varying(150) NULL;
        ALTER TABLE public."Beds" ADD COLUMN IF NOT EXISTS "StudentCode" character varying(50) NULL;
    """);
}

static string NormalizeConnectionString(string? rawConnectionString)
{
    if (string.IsNullOrWhiteSpace(rawConnectionString))
    {
        return string.Empty;
    }

    if (!rawConnectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        && !rawConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        return rawConnectionString;
    }

    var databaseUri = new Uri(rawConnectionString);
    var userInfo = databaseUri.UserInfo.Split(':', 2);
    var username = userInfo.Length > 0 ? WebUtility.UrlDecode(userInfo[0]) : string.Empty;
    var password = userInfo.Length > 1 ? WebUtility.UrlDecode(userInfo[1]) : string.Empty;

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.Port > 0 ? databaseUri.Port : 5432,
        Database = databaseUri.AbsolutePath.Trim('/'),
        Username = username,
        Password = password,
        SslMode = SslMode.Require,
        TrustServerCertificate = true
    };

    return builder.ConnectionString;
}

public partial class Program { }
