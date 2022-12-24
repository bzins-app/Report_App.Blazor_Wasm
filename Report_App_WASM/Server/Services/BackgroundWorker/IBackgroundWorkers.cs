using System.Net.Mail;
using Report_App_WASM.Server.Utils;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.SerializedParameters;

namespace Report_App_WASM.Server.Services.BackgroundWorker
{
    public interface IBackgroundWorkers
    {
        void SendEmail(List<EmailRecipient>? email, string subject, string message, List<Attachment> attachment = null);
        void DeleteFile(string filePath);
        Task SwitchBackgroundTasksPerActivityAsync(int activityId, bool activate);
        Task SwitchBackgroundTaskAsync(int taskHeaderId, bool activate);
        Task<SubmitResult> ActivateBackgroundWorkersAsync(bool activate, BackgroundTaskType type);
        void RunManuallyTask(int taskHeaderId, string runBy, List<EmailRecipient> emails, List<QueryCommandParameter> customQueryParameters, bool generateFiles = false);
    }
}
