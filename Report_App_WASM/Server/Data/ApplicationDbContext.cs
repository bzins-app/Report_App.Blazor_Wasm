using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Models;
using ZNetCS.AspNetCore.Logging.EntityFrameworkCore;

namespace Report_App_WASM.Server.Data;

public class ApplicationDbContext : AuditableIdentityContext
{
    public ApplicationDbContext(
        DbContextOptions options) : base(options)
    {
    }

    public virtual DbSet<ApplicationUser> ApplicationUser { get; set; } = null!;
    public virtual DbSet<ApplicationUniqueKey> ApplicationUniqueKey { get; set; } = null!;
    public virtual DbSet<ApplicationParameters> ApplicationParameters { get; set; } = null!;
    public virtual DbSet<ApplicationLogTask> ApplicationLogTask { get; set; } = null!;
    public virtual DbSet<ApplicationLogTaskDetails> ApplicationLogTaskDetails { get; set; } = null!;
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
    public virtual DbSet<SmtpConfiguration> SmtpConfiguration { get; set; } = null!;
    public virtual DbSet<LdapConfiguration> LdapConfiguration { get; set; } = null!;
    public virtual DbSet<FileDepositPathConfiguration> FileDepositPathConfiguration { get; set; } = null!;
    public virtual DbSet<SftpConfiguration> SftpConfiguration { get; set; } = null!;
    public virtual DbSet<QueryStore> QueryStore { get; set; } = null!;
    public virtual DbSet<DbTableDescriptions> DbTableDescriptions { get; set; } = null!;
    public virtual DbSet<UserSavedConfiguration> UserSavedConfiguration { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // build default model.
        LogModelBuilderHelper.Build(modelBuilder.Entity<ApplicationLogSystem>());

        //Entity relations and behaviours
        modelBuilder.Entity<Activity>()
            .HasMany(b => b.ActivityDbConnections)
            .WithOne(t => t.Activity).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Activity>()
            .HasMany(b => b.TaskHeaders)
            .WithOne(t => t.Activity).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Activity>()
            .HasMany(b => b.QueryStores)
            .WithOne(t => t.Activity).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ActivityDbConnection>()
            .HasMany(b => b.DbTableDescriptions)
            .WithOne(t => t.ActivityDbConnection).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TaskHeader>()
            .HasMany(b => b.TaskDetails)
            .WithOne(t => t.TaskHeader).IsRequired().OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<TaskHeader>()
            .HasMany(b => b.TaskEmailRecipients)
            .WithOne(t => t.TaskHeader).IsRequired().OnDelete(DeleteBehavior.Cascade);

        //custom indexes
        modelBuilder.Entity<ApplicationLogSystem>().HasIndex(r => r.TimeStampAppHour);
        modelBuilder.Entity<ApplicationLogSystem>().HasIndex(r => r.EventId);
        modelBuilder.Entity<ApplicationLogSystem>().HasIndex(r => r.Level);
        modelBuilder.Entity<ApplicationLogTask>().HasIndex(b => b.EndDateTime);
        modelBuilder.Entity<ApplicationLogTask>().HasIndex(b => new { b.Error, b.Result });
        modelBuilder.Entity<ApplicationLogEmailSender>().HasIndex(b => b.EndDateTime);
        modelBuilder.Entity<ApplicationLogEmailSender>().HasIndex(b => new { b.Error, b.Result });
        modelBuilder.Entity<ApplicationLogReportResult>().HasIndex(b => b.CreatedAt);
        modelBuilder.Entity<ApplicationLogReportResult>().HasIndex(b => b.IsAvailable);
        modelBuilder.Entity<ApplicationLogReportResult>().HasIndex(b => b.TaskHeaderId);
        modelBuilder.Entity<ApplicationLogReportResult>().HasIndex(b => new { b.ActivityId, b.ReportPath });
        modelBuilder.Entity<ApplicationAuditTrail>().HasIndex(b => b.DateTime);
        modelBuilder.Entity<ApplicationAuditTrail>().HasIndex(b => b.UserId);
        modelBuilder.Entity<ApplicationAuditTrail>().HasIndex(b => new { b.Type, b.TableName });
        modelBuilder.Entity<QueryStore>().HasIndex(b => new { b.QueryName });
        modelBuilder.Entity<ApplicationLogTaskDetails>().HasIndex(b => new { b.TaskId, b.Id });
        modelBuilder.Entity<DbTableDescriptions>().HasIndex(b => new { b.TableName, b.ColumnName });
        modelBuilder.Entity<UserSavedConfiguration>()
            .HasIndex(b => new { b.UserId, b.TypeConfiguration, b.IdIntConfiguration });
    }
}