using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WareHouse.Models;

namespace WareHouse;

public partial class NeondbContext : DbContext
{
    public NeondbContext()
    {
    }

    public NeondbContext(DbContextOptions<NeondbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<Saleitem> Saleitems { get; set; }

    public virtual DbSet<Shipment> Shipments { get; set; }

    public virtual DbSet<Shipmentitem> Shipmentitems { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=ep-mute-band-a8ch3ky8-pooler.eastus2.azure.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_SBV1YKWrxCf0");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Mail).HasName("accounts_pkey");

            entity.ToTable("accounts");

            entity.HasIndex(e => e.Userid, "accounts_userid_key").IsUnique();

            entity.Property(e => e.Mail)
                .HasMaxLength(50)
                .HasColumnName("mail");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Userid)
                .HasMaxLength(10)
                .HasColumnName("userid");

            entity.HasOne(d => d.User).WithOne(p => p.Account)
                .HasForeignKey<Account>(d => d.Userid)
                .HasConstraintName("fk_accounts_users");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("category_pkey");

            entity.ToTable("category");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("products_pkey");

            entity.ToTable("products");

            entity.Property(e => e.Id)
                .HasMaxLength(10)
                .HasColumnName("id");
            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.Categoryid)
                .HasConstraintName("products_categoryid_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sales_pkey");

            entity.ToTable("sales");

            entity.Property(e => e.Id)
                .HasMaxLength(10)
                .HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Userid)
                .HasMaxLength(10)
                .HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Sales)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("sales_userid_fkey");
        });

        modelBuilder.Entity<Saleitem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("saleitems_pkey");

            entity.ToTable("saleitems");

            entity.Property(e => e.Id)
                .HasMaxLength(10)
                .HasColumnName("id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Productid)
                .HasMaxLength(10)
                .HasColumnName("productid");
            entity.Property(e => e.Saleid)
                .HasMaxLength(10)
                .HasColumnName("saleid");

            entity.HasOne(d => d.Product).WithMany(p => p.Saleitems)
                .HasForeignKey(d => d.Productid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("saleitems_productid_fkey");

            entity.HasOne(d => d.Sale).WithMany(p => p.Saleitems)
                .HasForeignKey(d => d.Saleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("saleitems_saleid_fkey");
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shipments_pkey");

            entity.ToTable("shipments");

            entity.Property(e => e.Id)
                .HasMaxLength(10)
                .HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Statusid).HasColumnName("statusid");
            entity.Property(e => e.Userid)
                .HasMaxLength(10)
                .HasColumnName("userid");

            entity.HasOne(d => d.Status).WithMany(p => p.Shipments)
                .HasForeignKey(d => d.Statusid)
                .HasConstraintName("shipments_statusid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Shipments)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("shipments_userid_fkey");
        });

        modelBuilder.Entity<Shipmentitem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("shipmentitems_pkey");

            entity.ToTable("shipmentitems");

            entity.Property(e => e.Id)
                .HasMaxLength(10)
                .HasColumnName("id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Productid)
                .HasMaxLength(10)
                .HasColumnName("productid");
            entity.Property(e => e.Shipmentid)
                .HasMaxLength(10)
                .HasColumnName("shipmentid");

            entity.HasOne(d => d.Product).WithMany(p => p.Shipmentitems)
                .HasForeignKey(d => d.Productid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("shipmentitems_productid_fkey");

            entity.HasOne(d => d.Shipment).WithMany(p => p.Shipmentitems)
                .HasForeignKey(d => d.Shipmentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("shipmentitems_shipmentid_fkey");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("status_pkey");

            entity.ToTable("status");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Id)
                .HasMaxLength(10)
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Patronomic)
                .HasMaxLength(50)
                .HasColumnName("patronomic");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Surname)
                .HasMaxLength(50)
                .HasColumnName("surname");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .HasConstraintName("users_roleid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
