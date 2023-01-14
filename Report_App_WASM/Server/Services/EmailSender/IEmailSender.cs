using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils.EncryptDecrypt;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.Extensions;

namespace Report_App_WASM.Server.Services.EmailSender;

public interface IEmailSender
{
    Task<SubmitResult> SendEmailAsync(List<EmailRecipient>? email, string? subject, string message,
        List<Attachment>? attachment = null!);

    Task GenerateErrorEmailAsync(string? errorMessage, string subjectSuffix);
}

public class EmailSender : IEmailSender
{
    public EmailSender(ApplicationDbContext context)
    {
        Context = context;
    }

    private ApplicationDbContext Context { get; }

    public async Task GenerateErrorEmailAsync(string? errorMessage, string subjectSuffix)
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


    public async Task<SubmitResult> SendEmailAsync(List<EmailRecipient>? email, string? subject, string message,
        List<Attachment>? attachment = null!)

    {
        var smtp = await Context.SmtpConfiguration.Where(a => a.IsActivated == true).FirstOrDefaultAsync();
        var emailservice = await Context.ServicesStatus.Select(a => a.EmailService).FirstOrDefaultAsync();

        var result = new SubmitResult();
        //Smtp is become default

        if (email != null && smtp != null && email.Any() && emailservice)
        {
            ApplicationLogEmailSender log = new()
            {
                EmailTitle = subject, StartDateTime = DateTime.Now, NbrOfRecipients = email.Count,
                RecipientList = JsonSerializer.Serialize(email)
            };
            try
            {
                double size = 0;
                if (attachment != null)
                    foreach (var attach in attachment)
                        size = +BytesConverter.ConvertBytesToMegabytes(attach.ContentStream.Length);
                if (size > 20)
                {
                    attachment = null;
                    message = message + Environment.NewLine +
                              $"The size of the attachment is too high: {Math.Round(size, 2)}MB. Maximum is {20} ";
                }

                ProcessEmailAsync(smtp.FromEmail!,
                        smtp.FromFullName!,
                        subject,
                        message,
                        email,
                        email,
                        smtp.SmtpUserName!,
                        EncryptDecrypt.DecryptString(smtp.SmtpPassword),
                        smtp.SmtpHost,
                        smtp.SmtpPort,
                        smtp.SmtpSsl,
                        attachment!)
                    .Wait();

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
        List<EmailRecipient>? toEmail, List<EmailRecipient>? toFullName, string smtpUser, string? smtpPassword,
        string? smtpHost, int smtpPort, bool smtpSsl, List<Attachment>? attachment = null)

    {
        var message = new MailMessage();
        foreach (var t in toEmail!.Where(a => a.Bcc == false)) message.To.Add(new MailAddress(t.Email!, t.Email));

        foreach (var t in toEmail!.Where(a => a.Bcc)) message.Bcc.Add(new MailAddress(t.Email!, t.Email));
        message.From = new MailAddress(fromEmail, fromFullName);
        message.Subject = subject;
        message.Body = messageBody;

        if (attachment != null && attachment.Any())
            foreach (var item in attachment)
                message.Attachments.Add(item);

        message.IsBodyHtml = true;

        using var smtp = new SmtpClient();
        var credential = new NetworkCredential
        {
            UserName = smtpUser,
            Password = smtpPassword
        };
        smtp.Credentials = credential;

        smtp.Host = smtpHost!;

        smtp.Port = smtpPort;
        smtp.EnableSsl = smtpSsl;
        await smtp.SendMailAsync(message).ConfigureAwait(true);
    }
}