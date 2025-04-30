using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using TicketSystemAPI.Models;

namespace TicketSystemAPI.Data;

public partial class TicketSystemContext : DbContext
{
    public TicketSystemContext()
    {
    }

    public TicketSystemContext(DbContextOptions<TicketSystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Efmigrationshistory> Efmigrationshistories { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<Tickettype> Tickettypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=ticketsystem;user=root;password=root", ServerVersion.Parse("8.0.41-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Efmigrationshistory>(entity =>
        {
            entity.HasKey(e => e.MigrationId).HasName("PRIMARY");

            entity.ToTable("__efmigrationshistory");

            entity.Property(e => e.MigrationId).HasMaxLength(150);
            entity.Property(e => e.ProductVersion).HasMaxLength(32);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PRIMARY");

            entity
                .ToTable("payments")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.PaymentId, "paymentId_UNIQUE").IsUnique();

            entity.Property(e => e.PaymentId).HasColumnName("paymentId");
            entity.Property(e => e.Amount)
                .HasPrecision(10)
                .HasColumnName("amount");
            entity.Property(e => e.Method)
                .HasMaxLength(45)
                .HasColumnName("method");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PRIMARY");

            entity
                .ToTable("tickets")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.UserId, "fk_Tickets_1_idx");

            entity.HasIndex(e => e.TypeId, "fk_Tickets_3_idx");

            entity.HasIndex(e => e.PaymentId, "fk_Tickets_Payments");

            entity.HasIndex(e => e.TicketId, "ticketId_UNIQUE").IsUnique();

            entity.Property(e => e.TicketId).HasColumnName("ticketId");
            entity.Property(e => e.DiscountCode)
                .HasMaxLength(16)
                .HasColumnName("discountCode");
            entity.Property(e => e.ExpirationTime)
                .HasColumnType("datetime(1)")
                .HasColumnName("expirationTime");
            entity.Property(e => e.PaymentId).HasColumnName("paymentId");
            entity.Property(e => e.Price)
                .HasPrecision(10)
                .HasColumnName("price");
            entity.Property(e => e.PurchaseTime)
                .HasColumnType("datetime(1)")
                .HasColumnName("purchaseTime");
            entity.Property(e => e.RideLimit).HasColumnName("rideLimit");
            entity.Property(e => e.RidesTaken).HasColumnName("ridesTaken");
            entity.Property(e => e.TypeId).HasColumnName("typeId");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.Payment).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("fk_Tickets_Payments");

            entity.HasOne(d => d.Type).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Tickets_3");

            entity.HasOne(d => d.User).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Tickets_1");
        });

        modelBuilder.Entity<Tickettype>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PRIMARY");

            entity
                .ToTable("tickettypes")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.TypeId, "typeId_UNIQUE").IsUnique();

            entity.Property(e => e.TypeId).HasColumnName("typeId");
            entity.Property(e => e.BaseDurationDays).HasColumnName("baseDurationDays");
            entity.Property(e => e.BasePrice)
                .HasPrecision(10)
                .HasColumnName("basePrice");
            entity.Property(e => e.BaseRideLimit).HasColumnName("baseRideLimit");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity
                .ToTable("users")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.UserId, "userId_UNIQUE").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("userId");
            entity.Property(e => e.Email)
                .HasMaxLength(45)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(45)
                .HasColumnName("password");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
