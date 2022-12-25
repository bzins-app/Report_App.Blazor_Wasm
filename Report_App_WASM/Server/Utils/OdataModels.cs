using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Shared.DTO;

namespace Report_App_WASM.Server.Utils
{
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


            builder.EntitySet<LdapConfiguration>("Ldap");
            builder.EntitySet<SmtpConfiguration>("Smtp");
            builder.EntitySet<SftpConfiguration>("Sftp");
            builder.EntitySet<FileDepositPathConfiguration>("DepositPath");
            builder.EntitySet<Activity>("Activities");
            builder.EntitySet<TaskHeader>("TaskHeader");
            builder.EntitySet<ApplicationUser>("Users");

            builder.Action("ExtractLogs");
            return builder.GetEdmModel();
        }
    }
}
