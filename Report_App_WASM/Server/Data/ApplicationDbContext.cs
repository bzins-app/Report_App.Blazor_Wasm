﻿using ZNetCS.AspNetCore.Logging.EntityFrameworkCore;

namespace Report_App_WASM.Server.Data;

public class ApplicationDbContext : AuditableIdentityContext
{
    public ApplicationDbContext(
        DbContextOptions options) : base(options)
    {
    }

    public virtual DbSet<ApplicationUser> ApplicationUser { get; set; } = null!;
    public virtual DbSet<SystemUniqueKey> SystemUniqueKey { get; set; } = null!;
    public virtual DbSet<SystemParameters> SystemParameters { get; set; } = null!;
    public virtual DbSet<TaskLog> TaskLog { get; set; } = null!;
    public virtual DbSet<TaskStepLog> TaskStepLog { get; set; } = null!;
    public virtual DbSet<AdHocQueryExecutionLog> AdHocQueryExecutionLog { get; set; } = null!;
    public virtual DbSet<QueryExecutionLog> QueryExecutionLog { get; set; } = null!;
    public virtual DbSet<EmailLog> EmailLog { get; set; } = null!;
    public virtual DbSet<AuditTrail> AuditTrail { get; set; } = null!;
    public virtual DbSet<ReportGenerationLog> ReportGenerationLog { get; set; } = null!;
    public virtual DbSet<SystemLog> SystemLog { get; set; } = null!;
    public virtual DbSet<DataProvider> DataProvider { get; set; } = null!;
    public virtual DbSet<DatabaseConnection> DatabaseConnection { get; set; } = null!;
    public virtual DbSet<ScheduledTask> ScheduledTask { get; set; } = null!;
    public virtual DbSet<ScheduledTaskQuery> ScheduledTaskQuery { get; set; } = null!;
    public virtual DbSet<ScheduledTaskDistributionList> ScheduledTaskDistributionList { get; set; } = null!;
    public virtual DbSet<SystemServicesStatus> SystemServicesStatus { get; set; } = null!;
    public virtual DbSet<SmtpConfiguration> SmtpConfiguration { get; set; } = null!;
    public virtual DbSet<LdapConfiguration> LdapConfiguration { get; set; } = null!;
    public virtual DbSet<FileStorageLocation> FileStorageLocation { get; set; } = null!;
    public virtual DbSet<SftpConfiguration> SftpConfiguration { get; set; } = null!;
    public virtual DbSet<StoredQuery> StoredQuery { get; set; } = null!;
    public virtual DbSet<TableMetadata> TableMetadata { get; set; } = null!;
    public virtual DbSet<UserPreferences> UserPreferences { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // build default model.
        LogModelBuilderHelper.Build(modelBuilder.Entity<SystemLog>());

        //Entity relations and behaviours
        modelBuilder.Entity<DataProvider>()
            .HasMany(b => b.DatabaseConnection)
            .WithOne(t => t.DataProvider).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<DataProvider>()
            .HasMany(b => b.ScheduledTasks)
            .WithOne(t => t.DataProvider).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<DataProvider>()
            .HasMany(b => b.StoredQueries)
            .WithOne(t => t.DataProvider).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<DatabaseConnection>()
            .HasMany(b => b.TableMetadata)
            .WithOne(t => t.DatabaseConnection).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ScheduledTask>()
            .HasMany(b => b.TaskQueries)
            .WithOne(t => t.ScheduledTask).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ScheduledTask>()
            .HasMany(b => b.DistributionLists)
            .WithOne(t => t.ScheduledTask).IsRequired().OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationUser>(b => { b.ToTable("Users", schema: "auth"); });

        modelBuilder.Entity<IdentityUserClaim<Guid>>(b => { b.ToTable("UserClaims", schema: "auth"); });

        modelBuilder.Entity<IdentityUserLogin<Guid>>(b => { b.ToTable("UserLogins", schema: "auth"); });

        modelBuilder.Entity<IdentityUserToken<Guid>>(b => { b.ToTable("UserTokens", schema: "auth"); });

        modelBuilder.Entity<IdentityRole<Guid>>(b => { b.ToTable("Roles", schema: "auth"); });

        modelBuilder.Entity<IdentityRoleClaim<Guid>>(b => { b.ToTable("RoleClaims", schema: "auth"); });

        modelBuilder.Entity<IdentityUserRole<Guid>>(b => { b.ToTable("UserRoles", schema: "auth"); });

        //custom indexes
        modelBuilder.Entity<SystemLog>().HasIndex(r => r.TimeStampAppHour);
        modelBuilder.Entity<SystemLog>().HasIndex(r => r.EventId);
        modelBuilder.Entity<SystemLog>().HasIndex(r => r.Level);
        modelBuilder.Entity<TaskLog>().HasIndex(b => b.EndDateTime);
        modelBuilder.Entity<TaskLog>().HasIndex(b => new { b.Error, b.Result });
        modelBuilder.Entity<TaskStepLog>().HasIndex(b => new { b.Id, b.TimeStamp });
        modelBuilder.Entity<TaskStepLog>().HasIndex(b => new { b.TaskLogId, b.TimeStamp });
        modelBuilder.Entity<EmailLog>().HasIndex(b => b.EndDateTime);
        modelBuilder.Entity<EmailLog>().HasIndex(b => new { b.Error, b.Result });
        modelBuilder.Entity<ReportGenerationLog>().HasIndex(b => b.CreatedAt);
        modelBuilder.Entity<ReportGenerationLog>().HasIndex(b => b.IsAvailable);
        modelBuilder.Entity<ReportGenerationLog>().HasIndex(b => b.ScheduledTaskId);
        modelBuilder.Entity<ReportGenerationLog>().HasIndex(b => new { b.DataProviderId, b.ReportPath });
        modelBuilder.Entity<AuditTrail>().HasIndex(b => b.DateTime);
        modelBuilder.Entity<AuditTrail>().HasIndex(b => b.UserId);
        modelBuilder.Entity<AuditTrail>().HasIndex(b => new { b.Type, b.TableName });
        modelBuilder.Entity<StoredQuery>().HasIndex(b => new { b.QueryName });
        modelBuilder.Entity<TaskStepLog>().HasIndex(b => new { b.TaskLogId, b.Id });
        modelBuilder.Entity<TableMetadata>().HasIndex(b => new { b.TableName, b.ColumnName });
        modelBuilder.Entity<UserPreferences>()
            .HasIndex(b => new { b.UserId, b.TypeConfiguration, b.IdIntConfiguration });
        modelBuilder.Entity<AdHocQueryExecutionLog>()
            .HasIndex(b => new { b.DataProviderId, b.QueryId, b.JobDescription });
    }
}