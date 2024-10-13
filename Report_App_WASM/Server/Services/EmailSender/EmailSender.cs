using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace Report_App_WASM.Server.Services.EmailSender;

public class EmailSender : IEmailSender
{
    public EmailSender(ApplicationDbContext context)
    {
        Context = context;
    }

    private ApplicationDbContext Context { get; }

    public async Task GenerateErrorEmailAsync(string errorMessage, string subjectSuffix)
    {
        var emailInfos = await Context.ApplicationParameters
            .Select(a => new { a.ErrorEmailPrefix, a.ErrorEMailMessage, a.AdminEmails }).FirstOrDefaultAsync();

        if (emailInfos != null && emailInfos.AdminEmails != "[]")
        {
            var emails = JsonSerializer.Deserialize<List<EmailRecipient>>(emailInfos.AdminEmails!);
            var subject = $@"{emailInfos.ErrorEmailPrefix}-{subjectSuffix}";
            var messageMail = string.Format(emailInfos.ErrorEMailMessage!, errorMessage);
            await SendEmailAsync(emails, subject, messageMail);
        }
    }

    public async Task<SubmitResult> SendEmailAsync(List<EmailRecipient> email, string subject, string message,
        List<Attachment> attachment = null!)
    {
        var smtp = await Context.SmtpConfiguration.Where(a => a.IsActivated == true).AsNoTracking().FirstOrDefaultAsync();
        var emailservice = await Context.ServicesStatus.Select(a => a.EmailService).FirstOrDefaultAsync();

        var result = new SubmitResult();

        if (email != null && smtp != null && email.Any() && emailservice)
        {
            ApplicationLogEmailSender log = new()
            {
                EmailTitle = subject,
                StartDateTime = DateTime.Now,
                NbrOfRecipients = email.Count,
                RecipientList = JsonSerializer.Serialize(email)
            };
            try
            {
                double size = 0;
                if (attachment != null)
                {
                    foreach (var attach in attachment)
                    {
                        size += BytesConverter.ConvertBytesToMegabytes(attach.ContentStream.Length);
                    }
                }
                if (size > 20)
                {
                    attachment = null;
                    message += Environment.NewLine +
                               $"The size of the attachment is too high: {Math.Round(size, 2)}MB. Maximum is {20} ";
                }

                await ProcessEmailAsync(smtp.FromEmail!,
                        smtp.FromFullName!,
                        subject,
                        message,
                        email,
                        smtp.SmtpUserName!,
                        EncryptDecrypt.DecryptString(smtp.SmtpPassword),
                        smtp.SmtpHost,
                        smtp.SmtpPort,
                        smtp.SmtpSsl,
                        attachment);

                log.Result = "Ok";
                log.Error = false;
            }
            catch (Exception ex)
            {
                log.Result = ex.Message;
                log.Error = true;
                result.Success = false;
                result.Message = ex.Message;
            }

            log.EndDateTime = DateTime.Now;
            log.DurationInSeconds = (int)(log.EndDateTime - log.StartDateTime).TotalSeconds;
            await Context.AddAsync(log);
            await Context.SaveChangesAsync();
            result.Success = true;
        }
        else
        {
            result.Success = false;
            result.Message = "Smtp not configured or is not activated";
        }

        return result;
    }

    private async Task ProcessEmailAsync(string fromEmail, string fromFullName, string? subject, string messageBody,
        List<EmailRecipient>? toEmail, string smtpUser, string? smtpPassword,
        string? smtpHost, int smtpPort, bool smtpSsl, List<Attachment>? attachment = null)
    {
        var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromFullName),
            Subject = subject,
            Body = messageBody,
            IsBodyHtml = true
        };

        foreach (var recipient in toEmail!)
        {
            if (recipient.Bcc)
            {
                message.Bcc.Add(new MailAddress(recipient.Email, recipient.Email));
            }
            else
            {
                message.To.Add(new MailAddress(recipient.Email, recipient.Email));
            }
        }

        if (attachment != null && attachment.Any())
        {
            foreach (var item in attachment)
            {
                message.Attachments.Add(item);
            }
        }

        using var smtp = new SmtpClient
        {
            Credentials = new NetworkCredential(smtpUser, smtpPassword),
            Host = smtpHost!,
            Port = smtpPort,
            EnableSsl = smtpSsl
        };

        await smtp.SendMailAsync(message).ConfigureAwait(false);
    }
}
