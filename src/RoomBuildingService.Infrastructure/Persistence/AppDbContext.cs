namespace RoomBuildingService.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using RoomBuildingService.Core.Entities;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<RoomAmenity> RoomAmenities => Set<RoomAmenity>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Bed> Beds => Set<Bed>();
    public DbSet<RoomEquipment> RoomEquipments => Set<RoomEquipment>();
    public DbSet<BuildingFacility> BuildingFacilities => Set<BuildingFacility>();
    public DbSet<BuildingFacilityPricing> BuildingFacilityPricings => Set<BuildingFacilityPricing>();
    public DbSet<BuildingFacilityContract> BuildingFacilityContracts => Set<BuildingFacilityContract>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.HasDefaultSchema("public");

        mb.Entity<Building>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
            e.Property(x => x.GenderType).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("ACTIVE");

            e.HasCheckConstraint("CHK_Building_GenderType", "\"GenderType\" IN ('MALE', 'FEMALE', 'MIXED')");
            e.HasCheckConstraint("CHK_Building_Status", "\"Status\" IN ('ACTIVE', 'INACTIVE', 'UNDER_MAINTENANCE')");
            e.HasCheckConstraint("CHK_Building_TotalFloors", "\"TotalFloors\" > 0");
        });

        mb.Entity<BuildingFacility>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FacilityName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("ACTIVE");
            e.Property(x => x.Description).HasMaxLength(500);

            e.HasIndex(x => new { x.BuildingId, x.FacilityName }).IsUnique();
            e.HasCheckConstraint("CHK_BuildingFacility_Status", "\"Status\" IN ('ACTIVE', 'INACTIVE', 'UNDER_MAINTENANCE')");

            e.HasOne(x => x.Building)
                .WithMany(x => x.Facilities)
                .HasForeignKey(x => x.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<BuildingFacilityPricing>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Price).HasColumnType("numeric(18,2)");
            e.Property(x => x.BillingCycle).HasMaxLength(20).HasDefaultValue("FREE");
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("ACTIVE");
            e.Property(x => x.Description).HasMaxLength(500);

            e.HasIndex(x => new { x.FacilityId, x.BillingCycle, x.IsPaid }).IsUnique();
            e.HasCheckConstraint("CHK_BuildingFacilityPricing_Price", "\"Price\" >= 0");
            e.HasCheckConstraint("CHK_BuildingFacilityPricing_BillingCycle", "\"BillingCycle\" IN ('FREE', 'MONTHLY', 'YEARLY')");
            e.HasCheckConstraint("CHK_BuildingFacilityPricing_Status", "\"Status\" IN ('ACTIVE', 'INACTIVE')");

            e.HasOne(x => x.Facility)
                .WithMany(x => x.Pricings)
                .HasForeignKey(x => x.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<BuildingFacilityContract>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ContractCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.StudentName).HasMaxLength(150);
            e.Property(x => x.StudentCode).HasMaxLength(50);
            e.Property(x => x.ContractType).HasMaxLength(20).HasDefaultValue("MONTHLY");
            e.Property(x => x.TotalAmount).HasColumnType("numeric(18,2)");
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("ACTIVE");
            e.Property(x => x.Notes).HasMaxLength(500);

            e.HasIndex(x => x.ContractCode).IsUnique();
            e.HasIndex(x => new { x.FacilityId, x.Status });
            e.HasCheckConstraint("CHK_BuildingFacilityContract_Type", "\"ContractType\" IN ('MONTHLY', 'YEARLY')");
            e.HasCheckConstraint("CHK_BuildingFacilityContract_Status", "\"Status\" IN ('ACTIVE', 'EXPIRED', 'CANCELLED')");
            e.HasCheckConstraint("CHK_BuildingFacilityContract_TotalAmount", "\"TotalAmount\" >= 0");

            e.HasOne(x => x.Facility)
                .WithMany(x => x.Contracts)
                .HasForeignKey(x => x.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Pricing)
                .WithMany(x => x.Contracts)
                .HasForeignKey(x => x.PricingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<RoomType>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TypeName).HasMaxLength(50).IsRequired();
            e.Property(x => x.BasePrice).HasColumnType("numeric(18,2)");
            e.Property(x => x.ImageUrl);

            e.HasIndex(x => x.TypeName).IsUnique();
            e.HasCheckConstraint("CHK_RoomType_Capacity", "\"Capacity\" > 0");
            e.HasCheckConstraint("CHK_RoomType_BasePrice", "\"BasePrice\" >= 0");
        });

        mb.Entity<RoomAmenity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.AmenityName).HasMaxLength(100).IsRequired();

            e.HasIndex(x => new { x.RoomTypeId, x.AmenityName }).IsUnique();

            e.HasOne(x => x.RoomType)
                .WithMany(x => x.Amenities)
                .HasForeignKey(x => x.RoomTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<Room>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.RoomNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.ImageUrl);
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("AVAILABLE");
            e.Property(x => x.CurrentOccupancy).HasDefaultValue(0);

            e.HasIndex(x => new { x.BuildingId, x.RoomNumber }).IsUnique();
            e.HasIndex(x => new { x.BuildingId, x.Status });
            e.HasIndex(x => new { x.BuildingId, x.FloorNumber });

            e.HasCheckConstraint("CHK_Room_Status", "\"Status\" IN ('AVAILABLE', 'FULL', 'UNDER_MAINTENANCE', 'INACTIVE')");
            e.HasCheckConstraint("CHK_Room_FloorNumber", "\"FloorNumber\" > 0");
            e.HasCheckConstraint("CHK_Room_Occupancy", "\"CurrentOccupancy\" >= 0");

            e.HasOne(x => x.Building)
                .WithMany(x => x.Rooms)
                .HasForeignKey(x => x.BuildingId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.RoomType)
                .WithMany(x => x.Rooms)
                .HasForeignKey(x => x.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        mb.Entity<Bed>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.BedNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("AVAILABLE");
            e.Property(x => x.StudentName).HasMaxLength(150);
            e.Property(x => x.StudentCode).HasMaxLength(50);

            e.HasIndex(x => new { x.RoomId, x.BedNumber }).IsUnique();
            e.HasIndex(x => new { x.RoomId, x.Status });

            e.HasCheckConstraint("CHK_Bed_Status", "\"Status\" IN ('AVAILABLE', 'OCCUPIED', 'UNDER_MAINTENANCE', 'INACTIVE')");

            e.HasOne(x => x.Room)
                .WithMany(x => x.Beds)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<RoomEquipment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EquipmentName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Status).HasMaxLength(30).HasDefaultValue("ACTIVE");
            e.Property(x => x.EquipmentIndex).HasDefaultValue((short)1);

            e.HasIndex(x => new { x.RoomId, x.EquipmentName, x.EquipmentIndex }).IsUnique();
            e.HasIndex(x => new { x.RoomId, x.Status });

            e.HasCheckConstraint("CHK_Equipment_Status", "\"Status\" IN ('ACTIVE', 'UNDER_MAINTENANCE', 'BROKEN', 'RETIRED')");
            e.HasCheckConstraint("CHK_Equipment_Index", "\"EquipmentIndex\" > 0");

            e.HasOne(x => x.Room)
                .WithMany(x => x.Equipments)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        mb.Entity<OutboxEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EventType).HasMaxLength(100).IsRequired();
            e.Property(x => x.Payload).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("PENDING");
            e.Property(x => x.RetryCount).HasDefaultValue((short)0);

            e.HasCheckConstraint("CHK_Outbox_Status", "\"Status\" IN ('PENDING', 'SENT', 'FAILED')");

            e.HasIndex(x => x.Status)
                .HasFilter("\"Status\" = 'PENDING'")
                .HasDatabaseName("IX_OutboxEvents_Status");
        });
    }
}
