using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Models;

public partial class GAccountingDbContext : DbContext
{
    public GAccountingDbContext()
    {
    }

    public GAccountingDbContext(DbContextOptions<GAccountingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Action> Actions { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<DataAccessType> DataAccessTypes { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleAccess> RoleAccesses { get; set; }

    public virtual DbSet<RoleDataAccess> RoleDataAccesses { get; set; }

    public virtual DbSet<SessionMgr> SessionMgrs { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserInfo> UserInfos { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserSession> UserSessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=194.60.231.81:5432;Database=G_Accounting_DB;Username=postgres;Password=Maham@7796", x => x.UseNodaTime());
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Action>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Action_pkey");

            entity.ToTable("Action");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Path).HasColumnType("character varying");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Contact_pkey");

            entity.ToTable("Contact");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('public.SEQ_Contact')");
            entity.Property(e => e.Addresses).HasColumnType("json[]");
            entity.Property(e => e.Mobiles).HasColumnType("numeric(11,0)[]");
            entity.Property(e => e.Tells).HasColumnType("numeric(11,0)[]");
        });

        modelBuilder.Entity<DataAccessType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DataAccessType_pkey");

            entity.ToTable("DataAccessType");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Menu_pkey");

            entity.ToTable("Menu");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(100);
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Region_pkey");

            entity.ToTable("Region");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Rol_pkey");

            entity.ToTable("Role");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasColumnType("character varying");
        });

        modelBuilder.Entity<RoleAccess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("RoleAccess_pkey");

            entity.ToTable("RoleAccess");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<RoleDataAccess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("RoleDataAccess_pkey");

            entity.ToTable("RoleDataAccess");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<SessionMgr>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SessionMGR_pkey");

            entity.ToTable("SessionMGR");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Status_pkey");

            entity.ToTable("Status");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Caption).HasColumnType("character varying");
            entity.Property(e => e.Name).HasColumnType("character varying");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("User_pkey");

            entity.ToTable("User");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Mobile).HasPrecision(11);
            entity.Property(e => e.NationalCode).HasPrecision(10);
            entity.Property(e => e.Otpinfo)
                .HasColumnType("json")
                .HasColumnName("OTPInfo");
            entity.Property(e => e.Password).HasPrecision(100);
            entity.Property(e => e.UserName).HasMaxLength(50);

        });

        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserInfo_pkey");

            entity.ToTable("UserInfo");

            entity.Property(e => e.Id).HasDefaultValueSql("nextval('public.SEQ_UserInfo')");
            entity.Property(e => e.FatherName).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasColumnType("bit(1)");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.SedadInfo).HasColumnType("json");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserRol_pkey");

            entity.ToTable("UserRole");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserSession_pkey");

            entity.ToTable("UserSession");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.SessionInfo).HasColumnType("json");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
