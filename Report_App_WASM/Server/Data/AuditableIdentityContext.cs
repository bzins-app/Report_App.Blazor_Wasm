using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Report_App_WASM.Server.Data;

public abstract class AuditableIdentityContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AuditableIdentityContext(
        DbContextOptions options) : base(options)
    {
    }

    public DbSet<ApplicationAuditTrail> AuditLogs { get; set; } = null!;

    public virtual async Task<int> SaveChangesAsync(string? userId = null)
    {
        OnBeforeSaveChanges(userId);
        return await base.SaveChangesAsync();
    }

    private void OnBeforeSaveChanges(string? userId)
    {
        ChangeTracker.DetectChanges();
        var auditEntries = new List<AuditEntry>();
        var now = DateTime.Now;
        var defaultUserId = userId ?? "system";

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is BaseTraceability entity &&
                (entry.State == EntityState.Added || entry.State == EntityState.Modified))
            {
                if (entry.State == EntityState.Added)
                {
                    entity.CreateUser = defaultUserId;
                    entity.CreateDateTime = now;
                }

                entity.ModificationUser = defaultUserId;
                entity.ModDateTime = now;
            }

            if (entry.Entity is ApplicationAuditTrail || entry.State == EntityState.Detached ||
                entry.State == EntityState.Unchanged ||
                entry.Entity.GetType().GetInterfaces().Contains(typeof(IExcludeAuditTrail)))
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                TableName = entry.Entity.GetType().Name,
                UserId = userId
            };

            var oldProperties = entry.State == EntityState.Added ? entry.OriginalValues : entry.GetDatabaseValues();
            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;
                if (propertyName == "ModDateTime" || propertyName == "ModificationUser")
                    continue;

                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.AuditType = AuditType.Create;
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        auditEntry.AuditType = AuditType.Delete;
                        auditEntry.OldValues[propertyName] = oldProperties?[propertyName];
                        break;
                    case EntityState.Modified:
                        property.OriginalValue = oldProperties?[propertyName];
                        if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                        {
                            auditEntry.ChangedColumns.Add(propertyName);
                            auditEntry.AuditType = AuditType.Update;
                            auditEntry.OldValues[propertyName] = oldProperties?[propertyName];
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }
        }

        foreach (var auditEntry in auditEntries.Where(a => a.UserId != "backgroundworker"))
        {
            if (auditEntry.OldValues.Count > 0 || auditEntry.NewValues.Count > 0)
                AuditLogs.Add(auditEntry.ToAudit());
        }
    }
}
