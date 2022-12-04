using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Models.AuditModels;
using Report_App_WASM.Server.Models;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Report_App_WASM.Server.Data
{
    public abstract class AuditableIdentityContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AuditableIdentityContext(
            DbContextOptions options) : base(options)
        {
        }
        public DbSet<ApplicationAuditTrail> AuditLogs { get; set; }
        public virtual async Task<int> SaveChangesAsync(string userId = null)
        {
            OnBeforeSaveChanges(userId);
            var result = await base.SaveChangesAsync();
            return result;
        }
        private void OnBeforeSaveChanges(string userId)
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if ((entry.State == EntityState.Added || entry.State == EntityState.Modified) && (entry.Entity.GetType().BaseType == typeof(BaseTraceability)))
                {
                    var entity = entry.Entity as BaseTraceability;

                    if (entry.State == EntityState.Added)
                    {
                        entity.CreateUser = userId ?? "system";
                        entity.CreateDateTime = DateTime.Now;
                    }

                    entity.ModificationUser = userId ?? "system";
                    entity.ModDateTime = DateTime.Now;
                }

                if (entry.Entity is ApplicationAuditTrail || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged || entry.Entity.GetType().GetInterfaces().Contains(typeof(IExcludeAuditTrail)))
                    continue;
                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Entity.GetType().Name,
                    UserId = userId
                };
                var oldProperties = entry.State == EntityState.Added ? entry.OriginalValues : entry.GetDatabaseValues();
                auditEntries.Add(auditEntry);
                foreach (var property in entry.Properties.Where(a => a.Metadata.Name != "ModDateTime" && a.Metadata.Name != "ModificationUser"))
                {
                    string propertyName = property.Metadata.Name;
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
                            auditEntry.OldValues[propertyName] = oldProperties[propertyName];
                            break;
                        case EntityState.Modified:
                            property.OriginalValue = oldProperties[propertyName];
                            if (property.IsModified && property.OriginalValue != null ? !property.OriginalValue.Equals(property.CurrentValue) : property.CurrentValue != property.OriginalValue)
                            {
                                auditEntry.ChangedColumns.Add(propertyName);
                                auditEntry.AuditType = AuditType.Update;
                                auditEntry.OldValues[propertyName] = oldProperties[propertyName];
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }
            foreach (var auditEntry in auditEntries.Where(a => a.UserId != "backgroundworker"))
            {
                if (auditEntry.OldValues.Count > 0 || auditEntry.NewValues.Count > 0)
                {
                    AuditLogs.Add(auditEntry.ToAudit());
                }
            }
        }
    }
}
