using Microsoft.EntityFrameworkCore;
using System;
using v2.Models;

namespace v2.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserProfile> Users { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<ParkingLot> ParkingLots { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<ParkingSession> ParkingSessions { get; set; } = null!;


        // Billing can be computed from Payments, so no DbSet for Billing

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USER
            modelBuilder.Entity<UserProfile>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<UserProfile>()
                .HasMany<Vehicle>()
                .WithOne()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserProfile>()
                .HasMany<Reservation>()
                .WithOne()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // VEHICLE
            modelBuilder.Entity<Vehicle>()
                .HasKey(v => v.Id);

            modelBuilder.Entity<Vehicle>()
                .HasMany<Reservation>()
                .WithOne()
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // PARKING LOT
            modelBuilder.Entity<ParkingLot>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<ParkingLot>()
                .HasMany<Reservation>()
                .WithOne()
                .HasForeignKey(r => r.ParkingLotId)
                .OnDelete(DeleteBehavior.Restrict);

            // RESERVATION
            modelBuilder.Entity<Reservation>()
                .HasKey(r => r.Id);

            // PAYMENT
            modelBuilder.Entity<Payment>()
                .HasKey(p => p.Transaction);

            // No foreign key — CoupledTo is a simple string reference

            // OPTIONAL: Configure decimal precision for financial fields
            modelBuilder.Entity<Reservation>()
                .Property(r => r.Cost)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ParkingLot>()
                .Property(p => p.Tariff)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<ParkingLot>()
                .Property(p => p.DayTariff)
                .HasColumnType("decimal(18,2)");
        }
    }
}
