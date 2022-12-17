using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Report_App_WASM.Server.Models;

namespace Report_App_WASM.Server.Utils
{
    public static class OdataModels
    {
        public static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new();
            builder.EntitySet<ApplicationLogSystem>("SystemLogs");
            builder.EntitySet<ApplicationLogEmailSender>("EmailLogs");
            builder.EntitySet<ApplicationLogQueryExecution>("QueryExecutionLogs");
            builder.EntitySet<ApplicationLogReportResult>("ReportResultLogs");
            builder.EntitySet<ApplicationLogTask>("TaskLogs");
            builder.EntitySet<ApplicationAuditTrail>("AuditTrail");


            builder.EntitySet<LDAPConfiguration>("LDAP");
            builder.EntitySet<SMTPConfiguration>("SMTP");
            builder.EntitySet<SFTPConfiguration>("SFTP");
            builder.EntitySet<FileDepositPathConfiguration>("DepositPath");
            builder.EntitySet<Activity>("Activities");


            builder.Action("ExtractLogs");
            return builder.GetEdmModel();
        }
    }
}
