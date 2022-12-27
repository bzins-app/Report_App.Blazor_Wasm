using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils.EncryptDecrypt;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.Extensions;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace Report_App_WASM.Server.Services.EmailSender
{
    public interface IEmailSender
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Task<SubmitResult> SendEmailAsync(List<EmailRecipient>? email, string? subject, string message, List<Attachment> attachment = null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        Task GenerateErrorEmailAsync(string? errorMessage, string subjectSuffix);
    }
    public class EmailSender : IEmailSender
    {
        private ApplicationDbContext Context { get; }
        public EmailSender(ApplicationDbContext context)
        {
            Context = context;
        }

        public async Task GenerateErrorEmailAsync(string? errorMessage, string subjectSuffix)
        {

            var emailInfos = await Context.ApplicationParameters.Select(a => new { a.ErrorEmailPrefix, a.ErrorEMailMessage, a.AdminEmails }).FirstOrDefaultAsync();

            if (emailInfos != null && emailInfos.AdminEmails != "[]")
            {
#pragma warning disable CS8604 // Possible null reference argument for parameter 'json' in 'List<EmailRecipient>? JsonSerializer.Deserialize<List<EmailRecipient>>(string json, JsonSerializerOptions? options = null)'.
                List<EmailRecipient>? emails = JsonSerializer.Deserialize<List<EmailRecipient>>(emailInfos.AdminEmails);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'json' in 'List<EmailRecipient>? JsonSerializer.Deserialize<List<EmailRecipient>>(string json, JsonSerializerOptions? options = null)'.
                var subject = $@"{emailInfos.ErrorEmailPrefix}-{subjectSuffix}";
                var messageMail = string.Format(emailInfos.ErrorEMailMessage!, errorMessage);
                await SendEmailAsync(emails, subject, messageMail);
            }
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public async Task<SubmitResult> SendEmailAsync(List<EmailRecipient>? email, string? subject, string message, List<Attachment> attachment = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
            var smtp = await Context.SmtpConfiguration.Where(a => a.IsActivated == true).FirstOrDefaultAsync();
            var emailservice = await Context.ServicesStatus.Select(a => a.EmailService).FirstOrDefaultAsync();

            var result = new SubmitResult();
            //Smtp is become default
#pragma warning disable CS8604 // Possible null reference argument for parameter 'source' in 'bool Enumerable.Any<EmailRecipient>(IEnumerable<EmailRecipient> source)'.
            if (smtp != null && email.Any() && emailservice)
            {

                ApplicationLogEmailSender log = new() { EmailTitle = subject, StartDateTime = DateTime.Now, NbrOfRecipients = email.Count, RecipientList = JsonSerializer.Serialize(email) };
                try
                {
                    double size = 0;
                    if (attachment != null)
                    {
                        foreach (var attach in attachment)
                        {
                            size = +BytesConverter.ConvertBytesToMegabytes(attach.ContentStream.Length);
                        }
                    }
                    if (size > 20)
                    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        attachment = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
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
#pragma warning restore CS8604 // Possible null reference argument for parameter 'source' in 'bool Enumerable.Any<EmailRecipient>(IEnumerable<EmailRecipient> source)'.



            return result;
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private async Task ProcessEmailAsync(string fromEmail, string fromFullName, string? subject, string messageBody, List<EmailRecipient>? toEmail, List<EmailRecipient>? toFullName, string smtpUser, string? smtpPassword, string? smtpHost, int smtpPort, bool smtpSsl, List<Attachment> attachment = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
            var body = messageBody;
            var message = new MailMessage();
            foreach (var t in toEmail!.Where(a => a.Bcc == false))
            {
#pragma warning disable CS8604 // Possible null reference argument for parameter 'address' in 'MailAddress.MailAddress(string address, string? displayName)'.
                message.To.Add(new MailAddress(t.Email, t.Email));
#pragma warning restore CS8604 // Possible null reference argument for parameter 'address' in 'MailAddress.MailAddress(string address, string? displayName)'.
            }

            foreach (var t in toEmail!.Where(a => a.Bcc))
            {
#pragma warning disable CS8604 // Possible null reference argument for parameter 'address' in 'MailAddress.MailAddress(string address, string? displayName)'.
                message.Bcc.Add(new MailAddress(t.Email, t.Email));
#pragma warning restore CS8604 // Possible null reference argument for parameter 'address' in 'MailAddress.MailAddress(string address, string? displayName)'.
            }
            message.From = new(fromEmail, fromFullName);
            message.Subject = subject;
            message.Body = body;

            if (attachment != null && attachment.Any())
            {
                foreach (var item in attachment)
                {
                    message.Attachments.Add(item);
                }
            }

            message.IsBodyHtml = true;

            using var smtp = new SmtpClient();
            var credential = new NetworkCredential
            {
                UserName = smtpUser,
                Password = smtpPassword
            };
            smtp.Credentials = credential;
#pragma warning disable CS8601 // Possible null reference assignment.
            smtp.Host = smtpHost;
#pragma warning restore CS8601 // Possible null reference assignment.
            smtp.Port = smtpPort;
            smtp.EnableSsl = smtpSsl;
            await smtp.SendMailAsync(message).ConfigureAwait(true);

        }

    }
}
