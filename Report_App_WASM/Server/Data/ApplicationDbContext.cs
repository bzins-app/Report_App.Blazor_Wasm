using ZNetCS.AspNetCore.Logging.EntityFrameworkCore;

namespace Report_App_WASM.Server.Data;

public class ApplicationDbContext : AuditableIdentityContext
{
    public ApplicationDbContext(
        DbContextOptions options) : base(options)
    {
    }

    public virtual DbSet<ApplicationUniqueKey> ApplicationUniqueKey { get; set; } = null!;
    public virtual DbSet<ApplicationParameters> ApplicationParameters { get; set; } = null!;
    public virtual DbSet<ApplicationLogTask> ApplicationLogTask { get; set; } = null!;
    public virtual DbSet<ApplicationLogTaskDetails> ApplicationLogTaskDetails { get; set; } = null!;
    public virtual DbSet<ApplicationLogAdHocQueries> ApplicationLogAdHocQueries { get; set; } = null!;
    public virtual DbSet<ApplicationLogQueryExecution> ApplicationLogQueryExecution { get; set; } = null!;
    public virtual DbSet<ApplicationLogEmailSender> ApplicationLogEmailSender { get; set; } = null!;
    public virtual DbSet<ApplicationAuditTrail> ApplicationAuditTrail { get; set; } = null!;
    public virtual DbSet<ApplicationLogReportResult> ApplicationLogReportResult { get; set; } = null!;
    public virtual DbSet<ApplicationLogSystem> ApplicationLogSystem { get; set; } = null!;
    public virtual DbSet<Activity> Activity { get; set; } = null!;
    public virtual DbSet<ActivityDbConnection> ActivityDbConnection { get; set; } = null!;
    public virtual DbSet<TaskHeader> TaskHeader { get; set; } = null!;
    public virtual DbSet<TaskDetail> TaskDetail { get; set; } = null!;
    public virtual DbSet<TaskEmailRecipient> TaskEmailRecipient { get; set; } = null!;
    public virtual DbSet<ServicesStatus> ServicesStatus { get; set; } = null!;
    public virtual DbSet<FileDepositPathConfiguration> FileDepositPathConfiguration { get; set; } = null!;
    public virtual DbSet<QueryStore> QueryStore { get; set; } = null!;
    public virtual DbSet<DbTableDescriptions> DbTableDescriptions { get; set; } = null!;
    public virtual DbSet<UserSavedConfiguration> UserSavedConfiguration { get; set; } = null!;

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
        LogModelBuilderHelper.Build(modelBuilder.Entity<ApplicationLogSystem>());

        //Entity relations and behaviours
        modelBuilder.Entity<DataProvider>()
            .HasMany(b => b.DatabaseConnections)
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

        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Users");
        });

        modelBuilder.Entity<IdentityUserClaim<System.Guid>>(b =>
        {
            b.ToTable("UserClaims");
        });

        modelBuilder.Entity<IdentityUserLogin<System.Guid>>(b =>
        {
            b.ToTable("UserLogins");
        });

        modelBuilder.Entity<IdentityUserToken<System.Guid>>(b =>
        {
            b.ToTable("UserTokens");
        });

        modelBuilder.Entity<IdentityRole<System.Guid>>(b =>
        {
            b.ToTable("Roles");
        });

        modelBuilder.Entity<IdentityRoleClaim<System.Guid>>(b =>
        {
            b.ToTable("RoleClaims");
        });

        modelBuilder.Entity<IdentityUserRole<System.Guid>>(b =>
        {
            b.ToTable("UserRoles");
        });

        //custom indexes
        modelBuilder.Entity<SystemLog>().HasIndex(r => r.TimeStampAppHour);
        modelBuilder.Entity<SystemLog>().HasIndex(r => r.EventId);
        modelBuilder.Entity<SystemLog>().HasIndex(r => r.Level);
        modelBuilder.Entity<TaskLog>().HasIndex(b => b.EndDateTime);
        modelBuilder.Entity<TaskLog>().HasIndex(b => new { b.Error, b.Result });
        modelBuilder.Entity<TaskStepLog>().HasIndex(b => new { b.TaskId, b.TimeStamp });
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
        modelBuilder.Entity<TaskStepLog>().HasIndex(b => new { b.TaskId, b.Id });
        modelBuilder.Entity<TableMetadata>().HasIndex(b => new { b.TableName, b.ColumnName });
        modelBuilder.Entity<UserPreferences>()
            .HasIndex(b => new { b.UserId, b.TypeConfiguration, b.IdIntConfiguration });
        modelBuilder.Entity<AdHocQueryExecutionLog>()
            .HasIndex(b => new { b.DataProviderId, b.QueryId, b.JobDescription });
    }
}