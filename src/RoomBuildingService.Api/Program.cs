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
var useInMemoryDatabase =
    builder.Configuration.GetValue<bool>("UseInMemoryDatabase")
    || builder.Environment.IsEnvironment("Testing");
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
builder.Services.AddScoped<IBuildingFacilityRepository, BuildingFacilityRepository>();
builder.Services.AddScoped<IBuildingFacilityPricingRepository, BuildingFacilityPricingRepository>();
builder.Services.AddScoped<IBuildingFacilityContractRepository, BuildingFacilityContractRepository>();

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
    if (!useInMemoryDatabase)
    {
        db.Database.EnsureCreated();
        EnsureBedIntegrationColumns(db.Database);
        EnsureBuildingFacilitySchema(db.Database);
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

static void EnsureBuildingFacilitySchema(DatabaseFacade database)
{
    database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS public."BuildingFacilities" (
            "Id" uuid PRIMARY KEY,
            "BuildingId" uuid NOT NULL,
            "FacilityName" character varying(100) NOT NULL,
            "Status" character varying(30) NOT NULL DEFAULT 'ACTIVE',
            "Description" character varying(500) NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            "UpdatedAt" timestamp with time zone NULL
        );

        CREATE UNIQUE INDEX IF NOT EXISTS "IX_BuildingFacilities_BuildingId_FacilityName"
            ON public."BuildingFacilities" ("BuildingId", "FacilityName");

        CREATE TABLE IF NOT EXISTS public."BuildingFacilityPricings" (
            "Id" uuid PRIMARY KEY,
            "FacilityId" uuid NOT NULL,
            "IsPaid" boolean NOT NULL,
            "Price" numeric(18,2) NOT NULL DEFAULT 0,
            "BillingCycle" character varying(20) NOT NULL DEFAULT 'FREE',
            "Status" character varying(20) NOT NULL DEFAULT 'ACTIVE',
            "Description" character varying(500) NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            "UpdatedAt" timestamp with time zone NULL
        );

        CREATE UNIQUE INDEX IF NOT EXISTS "IX_BuildingFacilityPricings_FacilityId_BillingCycle_IsPaid"
            ON public."BuildingFacilityPricings" ("FacilityId", "BillingCycle", "IsPaid");

        CREATE TABLE IF NOT EXISTS public."BuildingFacilityContracts" (
            "Id" uuid PRIMARY KEY,
            "FacilityId" uuid NOT NULL,
            "PricingId" uuid NOT NULL,
            "ContractCode" character varying(50) NOT NULL,
            "StudentId" uuid NULL,
            "StudentName" character varying(150) NULL,
            "StudentCode" character varying(50) NULL,
            "ContractType" character varying(20) NOT NULL DEFAULT 'MONTHLY',
            "StartDate" date NOT NULL,
            "EndDate" date NOT NULL,
            "TotalAmount" numeric(18,2) NOT NULL DEFAULT 0,
            "Status" character varying(20) NOT NULL DEFAULT 'ACTIVE',
            "Notes" character varying(500) NULL,
            "CreatedAt" timestamp with time zone NOT NULL,
            "UpdatedAt" timestamp with time zone NULL
        );

        CREATE UNIQUE INDEX IF NOT EXISTS "IX_BuildingFacilityContracts_ContractCode"
            ON public."BuildingFacilityContracts" ("ContractCode");
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
