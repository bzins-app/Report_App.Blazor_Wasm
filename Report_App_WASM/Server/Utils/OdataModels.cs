using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Report_App_WASM.Shared.DTO;

namespace Report_App_WASM.Server.Utils;

public static class OdataModels
{
    public static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new();
        builder.EntitySet<SystemLog>("SystemLogs");
        builder.EntitySet<EmailLog>("EmailLogs");
        builder.EntitySet<QueryExecutionLogDto>("QueryExecutionLogs");
        builder.EntitySet<ReportGenerationLog>("ReportResultLogs");
        builder.EntitySet<TaskLog>("TaskLogs");
        builder.EntitySet<AuditTrail>("AuditTrail");
        builder.EntitySet<AdHocQueryExecutionLog>("QueriesLogs");

        builder.EntitySet<LdapConfiguration>("Ldap");
        builder.EntitySet<SmtpConfiguration>("Smtp");
        builder.EntitySet<SftpConfiguration>("Sftp");
        builder.EntitySet<FileStorageLocationDto>("DepositPath");
        builder.EntitySet<DataProvider>("Activities");
        builder.EntitySet<DataProvider>("DataTransfers");
        builder.EntitySet<ScheduledTask>("TaskHeader");
        builder.EntitySet<ApplicationUserDto>("Users");
        builder.EntitySet<StoredQuery>("Queries");
        builder.EntitySet<UsersPerRole>("UsersRole");

        builder.Action("ExtractLogs");
        return builder.GetEdmModel();
    }
}