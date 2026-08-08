using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WeddingBookingApplication.Database.AppDbContextModels;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblBooking> TblBookings { get; set; }

    public virtual DbSet<TblBookingDecoration> TblBookingDecorations { get; set; }

    public virtual DbSet<TblBookingService> TblBookingServices { get; set; }

    public virtual DbSet<TblDecorationPackage> TblDecorationPackages { get; set; }

    public virtual DbSet<TblServicePackage> TblServicePackages { get; set; }

    public virtual DbSet<TblVendor> TblVendors { get; set; }

    public virtual DbSet<TblVenue> TblVenues { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-1LF20QJ8\\SQLEXPRESS;Database=WeddingBookingDb;User ID=sa;Password=sasa@123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblBooking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__TblBooki__73951AED4CD2CA5D");

            entity.ToTable("TblBooking");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerEmail).HasMaxLength(100);
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.CustomerPhone).HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Vendor).WithMany(p => p.TblBookings)
                .HasForeignKey(d => d.VendorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Booking_Vendor");

            entity.HasOne(d => d.Venue).WithMany(p => p.TblBookings)
                .HasForeignKey(d => d.VenueId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Booking_Venue");
        });

        modelBuilder.Entity<TblBookingDecoration>(entity =>
        {
            entity.HasKey(e => e.BookingDecorationId).HasName("PK__TblBooki__C5C02CA76DEAD7F3");

            entity.ToTable("TblBookingDecoration");

            entity.HasOne(d => d.Booking).WithMany(p => p.TblBookingDecorations)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingDecoration_Booking");

            entity.HasOne(d => d.DecorationPackage).WithMany(p => p.TblBookingDecorations)
                .HasForeignKey(d => d.DecorationPackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingDecoration_DecorationPackage");
        });

        modelBuilder.Entity<TblBookingService>(entity =>
        {
            entity.HasKey(e => e.BookingServiceId).HasName("PK__TblBooki__43F55CB11CE7F6CA");

            entity.ToTable("TblBookingService");

            entity.HasOne(d => d.Booking).WithMany(p => p.TblBookingServices)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingService_Booking");

            entity.HasOne(d => d.ServicePackage).WithMany(p => p.TblBookingServices)
                .HasForeignKey(d => d.ServicePackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingService_ServicePackage");
        });

        modelBuilder.Entity<TblDecorationPackage>(entity =>
        {
            entity.HasKey(e => e.DecorationPackageId).HasName("PK__TblDecor__10FC6471BBE66ABA");

            entity.ToTable("TblDecorationPackage");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PackageName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Vendor).WithMany(p => p.TblDecorationPackages)
                .HasForeignKey(d => d.VendorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DecorationPackage_Vendor");
        });

        modelBuilder.Entity<TblServicePackage>(entity =>
        {
            entity.HasKey(e => e.ServicePackageId).HasName("PK__TblServi__0747A82F80DE3B1A");

            entity.ToTable("TblServicePackage");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PackageName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Vendor).WithMany(p => p.TblServicePackages)
                .HasForeignKey(d => d.VendorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServicePackage_Vendor");
        });

        modelBuilder.Entity<TblVendor>(entity =>
        {
            entity.HasKey(e => e.VendorId).HasName("PK__TblVendo__FC8618F3CF7AECA0");

            entity.ToTable("TblVendor");

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.VendorName).HasMaxLength(100);
        });

        modelBuilder.Entity<TblVenue>(entity =>
        {
            entity.HasKey(e => e.VenueId).HasName("PK__TblVenue__3C57E5F240A416E4");

            entity.ToTable("TblVenue");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.VenueName).HasMaxLength(100);

            entity.HasOne(d => d.Vendor).WithMany(p => p.TblVenues)
                .HasForeignKey(d => d.VendorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Venue_Vendor");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
