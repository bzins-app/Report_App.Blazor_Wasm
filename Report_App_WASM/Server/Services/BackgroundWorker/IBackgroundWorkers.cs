using Report_App_WASM.Shared;
using Report_App_WASM.Shared.SerializedParameters;
using System.Net.Mail;

namespace ReportAppWASM.Server.Services.BackgroundWorker
{
    public interface IBackgroundWorkers
    {
        void SendEmail(List<EmailRecipient> email, string subject, string message, List<Attachment> Attachment = null);
        void DeleteFile(string filePath);
        Task SwitchBackgroundTasksPerActivityAsync(int activityId, bool activate);
        Task SwitchBackgroundTaskAsync(int taskHeaderId, bool activate);
        Task<SubmitResult> ActivateBackgroundWorkersAsync(bool activate, string type);
        void RunManuallyTask(int taskHeaderId, string runBy, List<EmailRecipient> emails, List<QueryCommandParameter> customQueryParameters, bool generateFiles = false);
    }
}
