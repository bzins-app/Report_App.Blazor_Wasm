using System.Net.Mail;

namespace Report_App_WASM.Server.Services.EmailSender;

public interface IEmailSender
{
    Task<SubmitResult> SendEmailAsync(List<EmailRecipient> email, string subject, string message,
        List<Attachment> attachment = null!);

    Task GenerateErrorEmailAsync(string errorMessage, string subjectSuffix);
}