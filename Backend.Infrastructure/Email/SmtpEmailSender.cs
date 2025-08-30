using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces.Infrastructure;
using Microsoft.Extensions.Options;
using System.Threading;

namespace Backend.Infrastructure.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public SmtpEmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task<EmailResult> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromAddress, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(new MailAddress(to));

                using var client = CreateSmtpClient();
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_emailSettings.TimeoutSeconds));
                
                // Use Task.Run to make it cancellable
                await Task.Run(async () =>
                {
                    await client.SendMailAsync(message);
                }, cts.Token);
                
                return new EmailResult 
                { 
                    IsSuccess = true,
                    MessageId = Guid.NewGuid().ToString() // Generate unique message ID
                };
            }
            catch (OperationCanceledException)
            {
                return new EmailResult 
                { 
                    IsSuccess = false,
                    ErrorMessage = $"SMTP request timed out after {_emailSettings.TimeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                return new EmailResult 
                { 
                    IsSuccess = false,
                    ErrorMessage = $"SMTP error: {ex.Message}"
                };
            }
        }

        public async Task<EmailResult> SendEmailWithAttachmentsAsync(string to, string subject, string body, List<string> attachmentPaths, bool isHtml = false)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromAddress, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };

                message.To.Add(new MailAddress(to));

                if (attachmentPaths != null && attachmentPaths.Any())
                {
                    foreach (var attachmentPath in attachmentPaths)
                    {
                        if (File.Exists(attachmentPath))
                        {
                            var attachment = new Attachment(attachmentPath);
                            message.Attachments.Add(attachment);
                        }
                    }
                }

                using var client = CreateSmtpClient();
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_emailSettings.TimeoutSeconds));
                
                // Use Task.Run to make it cancellable
                await Task.Run(async () =>
                {
                    await client.SendMailAsync(message);
                }, cts.Token);
                
                return new EmailResult 
                { 
                    IsSuccess = true,
                    MessageId = Guid.NewGuid().ToString() // Generate unique message ID
                };
            }
            catch (OperationCanceledException)
            {
                return new EmailResult 
                { 
                    IsSuccess = false,
                    ErrorMessage = $"SMTP request timed out after {_emailSettings.TimeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                return new EmailResult 
                { 
                    IsSuccess = false,
                    ErrorMessage = $"SMTP error: {ex.Message}"
                };
            }
        }

        private SmtpClient CreateSmtpClient()
        {
            var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                EnableSsl = _emailSettings.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                Timeout = _emailSettings.TimeoutSeconds * 1000 // Convert to milliseconds
            };

            return client;
        }
    }
}