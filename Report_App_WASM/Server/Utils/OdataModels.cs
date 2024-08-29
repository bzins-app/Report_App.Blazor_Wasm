using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Report_App_WASM.Shared.DTO;

namespace Report_App_WASM.Server.Utils;

public static class OdataModels
{
    public static IEdmModel GetEdmModel()
    {
        ODataConventionModelBuilder builder = new();
        builder.EntitySet<ApplicationLogSystem>("SystemLogs");
        builder.EntitySet<ApplicationLogEmailSender>("EmailLogs");
        builder.EntitySet<ApplicationLogQueryExecutionDto>("QueryExecutionLogs");
        builder.EntitySet<ApplicationLogReportResult>("ReportResultLogs");
        builder.EntitySet<ApplicationLogTask>("TaskLogs");
        builder.EntitySet<ApplicationAuditTrail>("AuditTrail");
        builder.EntitySet<ApplicationLogAdHocQueries>("QueriesLogs");

        builder.EntitySet<LdapConfiguration>("Ldap");
        builder.EntitySet<SmtpConfiguration>("Smtp");
        builder.EntitySet<SftpConfiguration>("Sftp");
        builder.EntitySet<FileDepositPathConfigurationDto>("DepositPath");
        builder.EntitySet<Activity>("Activities");
        builder.EntitySet<Activity>("DataTransfers");
        builder.EntitySet<TaskHeader>("TaskHeader");
        builder.EntitySet<ApplicationUserDto>("Users");
        builder.EntitySet<QueryStore>("Queries");
        builder.EntitySet<UsersPerRole>("UsersRole");

        builder.Action("ExtractLogs");
        return builder.GetEdmModel();
    }
}