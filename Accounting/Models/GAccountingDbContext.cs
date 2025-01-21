using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Accounting.Models;

public partial class GAccountingDbContext : DbContext
{
    private readonly IConfiguration _config;
    public GAccountingDbContext()
    {
        _config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
    }

    public GAccountingDbContext(DbContextOptions<GAccountingDbContext> options)
        : base(options)
    {
        _config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
    }
    public virtual DbSet<Action> Actions { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<DataAccessType> DataAccessTypes { get; set; }

    public virtual DbSet<LegalUserInfo> LegalUserInfos { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleAccess> RoleAccesses { get; set; }

    public virtual DbSet<RoleDataAccess> RoleDataAccesses { get; set; }

    public virtual DbSet<RoleStateAccess> RoleStateAccesses { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<SessionMgr> SessionMgrs { get; set; }

    public virtual DbSet<Setting> Settings { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserInfo> UserInfos { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    public virtual DbSet<UserSession> UserSessions { get; set; }

    public virtual DbSet<UserState> UserStates { get; set; }

    public virtual DbSet<UserStateType> UserStateTypes { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_config.GetConnectionString("GAccountingDbContext"), x => x.UseNodaTime());

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Action>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Action_pkey");

            entity.ToTable("Action", "archive");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Path).HasColumnType("character varying");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Contact_pkey");

            entity.ToTable("Contact");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Addresses).HasColumnType("jsonb");
        });

        modelBuilder.Entity<DataAccessType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("DataAccessType_pkey");

            entity.ToTable("DataAccessType");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<LegalUserInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("LegalUserInfo_pkey");

            entity.ToTable("LegalUserInfo");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(500);
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

        modelBuilder.Entity<RoleStateAccess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("RoleStateAccess_pkey");

            entity.ToTable("RoleStateAccess");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Service_pkey");

            entity.ToTable("Service");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AccessInfo).HasColumnType("json");
            entity.Property(e => e.Caption).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<SessionMgr>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SessionMGR_pkey");

            entity.ToTable("SessionMGR");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Setting_pkey");

            entity.ToTable("Setting");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Caption).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Nsme).HasMaxLength(100);
            entity.Property(e => e.Value).HasMaxLength(200);
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
            entity.Property(e => e.IdentificationCode).HasMaxLength(20);
            entity.Property(e => e.Otpinfo)
                .HasColumnType("json")
                .HasColumnName("OTPInfo");
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.ReferralCode).HasMaxLength(20);
            entity.Property(e => e.UserName).HasMaxLength(50);
            entity.Property(e => e.UserType).HasDefaultValue((short)100);
        });

        modelBuilder.Entity<UserInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserInfo_pkey");

            entity.ToTable("UserInfo");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.BirthDay).HasMaxLength(10);
            entity.Property(e => e.FatherName).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.NationalCardImage).HasColumnType("json");
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

            entity.Property(e => e.Id).UseIdentityAlwaysColumn()
                .HasIdentityOptions(100000000L, 1, 100000000L, 1000000000000000000L, null, 30L);
            entity.Property(e => e.Ip)
                .HasMaxLength(20)
                .HasColumnName("IP");
            entity.Property(e => e.SessionInfo).HasColumnType("json");
            entity.Property(e => e.Status).HasDefaultValue(0);
            entity.Property(e => e.UserId).HasDefaultValue(0L);
        });

        modelBuilder.Entity<UserState>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserStatte_pkey");

            entity.ToTable("UserState");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<UserStateType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserStateType_pkey");

            entity.ToTable("UserStateType");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasColumnType("character varying");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("UserType_pkey");

            entity.ToTable("UserType");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
