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
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ParkingLot>()
                .HasMany<ParkingSession>()
                .WithOne()
                .HasForeignKey(ps => ps.ParkingLotId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ParkingLot>()
                .HasMany<Payment>()
                .WithOne()
                .HasForeignKey(p => p.ParkingLotId)
                .OnDelete(DeleteBehavior.Cascade);

            // RESERVATION
            modelBuilder.Entity<Reservation>()
                .HasKey(r => r.Id);

            // PARKING SESSION
            modelBuilder.Entity<ParkingSession>()
                .HasKey(ps => ps.Id);

            // PAYMENT
            modelBuilder.Entity<Payment>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Id)
                .ValueGeneratedOnAdd(); //generate id

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.Transaction)
                .IsUnique();

            modelBuilder.Entity<Payment>()
                .OwnsOne(p => p.TData, td =>
                {
                    td.Property(t => t.Amount).HasColumnName("amount");
                    td.Property(t => t.Date).HasColumnName("date");
                    td.Property(t => t.Method).HasColumnName("method");
                    td.Property(t => t.Issuer).HasColumnName("issuer");
                    td.Property(t => t.Bank).HasColumnName("bank");
                });
        }
    }
}
