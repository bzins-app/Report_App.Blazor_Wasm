using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils.EncryptDecrypt;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.Extensions;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace ReportAppWASM.Server.Services.EmailSender
{
    public interface IEmailSender
    {
        Task<Task> SendEmailAsync(List<EmailRecipient> email, string subject, string message, List<Attachment> Attachment = null);
        Task GenerateErrorEmailAsync(string errorMessage, string subjectSuffix);
    }
    public class EmailSender : IEmailSender
    {
        private ApplicationDbContext _context { get; }
        public EmailSender(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task GenerateErrorEmailAsync(string errorMessage, string subjectSuffix)
        {
            var emailInfos = await _context.ApplicationParameters.Select(a => new { a.ErrorEmailPrefix, a.ErrorEMailMessage, a.AdminEmails }).FirstOrDefaultAsync();
            if (emailInfos != null && emailInfos.AdminEmails != "[]")
            {
                List<EmailRecipient> emails = JsonSerializer.Deserialize<List<EmailRecipient>>(emailInfos.AdminEmails);
                string subject = $@"{emailInfos.ErrorEmailPrefix}-{subjectSuffix}";
                string messageMail = string.Format(emailInfos.ErrorEMailMessage, errorMessage);
                await SendEmailAsync(emails, subject, messageMail);
            }
        }

        public async Task<Task> SendEmailAsync(List<EmailRecipient> email, string subject, string message, List<Attachment> Attachment = null)
        {
            var Smtp = await _context.SMTPConfiguration.Where(a => a.IsActivated == true).FirstOrDefaultAsync();
            var emailservice = await _context.ServicesStatus.Select(a => a.EmailService).FirstOrDefaultAsync();
            //smtp is become default
            if (Smtp != null && email.Any() && emailservice)
            {

                ApplicationLogEmailSender log = new() { EmailTitle = subject, StartDateTime = DateTime.Now, NbrOfRecipients = email.Count, RecipientList = JsonSerializer.Serialize(email) };
                try
                {
                    double size = 0;
                    if (Attachment != null)
                    {
                        foreach (var attach in Attachment)
                        {
                            size = +BytesConverter.ConvertBytesToMegabytes(attach.ContentStream.Length);
                        }
                    }
                    if (size > 20)
                    {
                        Attachment = null;
                        message = message + Environment.NewLine + string.Format("The size of the attachment is too high: {0}MB. Maximum is {1} ", Math.Round(size, 2), 20);
                    }

                    ProcessEmailAsync(Smtp.fromEmail,
                                                 Smtp.fromFullName,
                                                 subject,
                                                 message,
                                                 email,
                                                 email,
                                                 Smtp.smtpUserName,
                                                 EncryptDecrypt.DecryptString(Smtp.smtpPassword),
                                                 Smtp.smtpHost,
                                                 Smtp.smtpPort,
                                                 Smtp.smtpSSL,
                                                 Attachment)
                                                 .Wait();

                    log.Result = "Ok";
                    log.Error = false;
                }
                catch (Exception ex)
                {
                    log.Result = ex.Message.ToString();
                    log.Error = true;
                }
                log.EndDateTime = DateTime.Now;
                log.DurationInSeconds = (int)(log.EndDateTime - log.StartDateTime).TotalSeconds;
                await _context.AddAsync(log);
                await _context.SaveChangesAsync();
            }



            return Task.CompletedTask;
        }

        private async Task ProcessEmailAsync(string fromEmail, string fromFullName, string subject, string messageBody, List<EmailRecipient> toEmail, List<EmailRecipient> toFullName, string smtpUser, string smtpPassword, string smtpHost, int smtpPort, bool smtpSSL, List<Attachment> Attachment = null)
        {
            var body = messageBody;
            var message = new MailMessage();
            foreach (var t in toEmail.Where(a => a.BCC == false))
            {
                message.To.Add(new MailAddress(t.Email, t.Email));
            }

            foreach (var t in toEmail.Where(a => a.BCC))
            {
                message.Bcc.Add(new MailAddress(t.Email, t.Email));
            }
            message.From = new MailAddress(fromEmail, fromFullName);
            message.Subject = subject;
            message.Body = body;

            if (Attachment != null && Attachment.Any())
            {
                foreach (var item in Attachment)
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
            smtp.Host = smtpHost;
            smtp.Port = smtpPort;
            smtp.EnableSsl = smtpSSL;
            await smtp.SendMailAsync(message).ConfigureAwait(true);

        }

    }
}
