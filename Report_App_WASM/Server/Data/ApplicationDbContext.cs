using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Models;
using ZNetCS.AspNetCore.Logging.EntityFrameworkCore;

namespace Report_App_WASM.Server.Data
{
    public class ApplicationDbContext : AuditableIdentityContext
    {
        public ApplicationDbContext(
            DbContextOptions options) : base(options)
        {
        }

        public virtual DbSet<ApplicationUser> ApplicationUser { get; set; }
        public virtual DbSet<ApplicationUniqueKey> ApplicationUniqueKey { get; set; }
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.ApplicationParameters?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbSet<TEntity>'. Nullability of type argument 'Report_App_WASM.Server.Models.ApplicationParameters?' doesn't match 'class' constraint.
        public virtual DbSet<ApplicationParameters?> ApplicationParameters { get; set; }
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.ApplicationParameters?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbSet<TEntity>'. Nullability of type argument 'Report_App_WASM.Server.Models.ApplicationParameters?' doesn't match 'class' constraint.
        public virtual DbSet<ApplicationLogTask> ApplicationLogTask { get; set; }
        public virtual DbSet<ApplicationLogTaskDetails> ApplicationLogTaskDetails { get; set; }
        public virtual DbSet<ApplicationLogQueryExecution> ApplicationLogQueryExecution { get; set; }
        public virtual DbSet<ApplicationLogEmailSender> ApplicationLogEmailSender { get; set; }
        public virtual DbSet<ApplicationAuditTrail> ApplicationAuditTrail { get; set; }
        public virtual DbSet<ApplicationLogReportResult> ApplicationLogReportResult { get; set; }
        public virtual DbSet<ApplicationLogSystem> ApplicationLogSystem { get; set; }
        public virtual DbSet<Activity> Activity { get; set; }
        public virtual DbSet<ActivityDbConnection> ActivityDbConnection { get; set; }
        public virtual DbSet<TaskHeader> TaskHeader { get; set; }
        public virtual DbSet<TaskDetail> TaskDetail { get; set; }
        public virtual DbSet<TaskEmailRecipient> TaskEmailRecipient { get; set; }
        public virtual DbSet<ServicesStatus> ServicesStatus { get; set; }
        public virtual DbSet<SmtpConfiguration> SmtpConfiguration { get; set; }
        public virtual DbSet<LdapConfiguration> LdapConfiguration { get; set; }
        public virtual DbSet<FileDepositPathConfiguration> FileDepositPathConfiguration { get; set; }
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.SftpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbSet<TEntity>'. Nullability of type argument 'Report_App_WASM.Server.Models.SftpConfiguration?' doesn't match 'class' constraint.
        public virtual DbSet<SftpConfiguration?> SftpConfiguration { get; set; }
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.SftpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbSet<TEntity>'. Nullability of type argument 'Report_App_WASM.Server.Models.SftpConfiguration?' doesn't match 'class' constraint.
        public virtual DbSet<QueryStore> QueryStore { get; set; }

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
            modelBuilder.Entity<QueryStore>().HasIndex(b => new { b.Typoplogy, b.Area });
            modelBuilder.Entity<ApplicationLogTaskDetails>().HasIndex(b => new { b.TaskId, b.Id });
        }
    }
}