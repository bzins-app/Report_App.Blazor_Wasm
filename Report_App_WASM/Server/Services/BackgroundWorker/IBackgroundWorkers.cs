using System.Net.Mail;
using Report_App_WASM.Server.Utils.BackgroundWorker;

namespace Report_App_WASM.Server.Services.BackgroundWorker;

public interface IBackgroundWorkers
{
    void SendEmail(List<EmailRecipient>? email, string? subject, string message, List<Attachment>? attachment = null);

    void DeleteFile(string filePath);
    Task SwitchBackgroundTasksPerActivityAsync(long activityId, bool activate);
    Task SwitchBackgroundTaskAsync(long taskHeaderId, bool activate);
    Task<SubmitResult> ActivateBackgroundWorkersAsync(bool activate, BackgroundTaskType type);

    void RunManuallyTask(long taskHeaderId, string? runBy, List<EmailRecipient> emails,
        List<QueryCommandParameter> customQueryParameters, bool generateFiles = false);
}