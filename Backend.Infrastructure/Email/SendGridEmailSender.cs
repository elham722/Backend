using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces.Infrastructure;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Backend.Infrastructure.Email
{
    public class SendGridEmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public SendGridEmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task<EmailResult> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                var client = new SendGridClient(_emailSettings.ApiKey);
                
                var from = new EmailAddress(_emailSettings.FromAddress, _emailSettings.FromName);
                var toAddress = new EmailAddress(to);
                var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, isHtml ? string.Empty : body, isHtml ? body : string.Empty);
                
                // Use CancellationToken for timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_emailSettings.TimeoutSeconds));
                var response = await client.SendEmailAsync(msg, cts.Token);
                
                if (response.IsSuccessStatusCode)
                {
                    return new EmailResult 
                    { 
                        IsSuccess = true,
                        MessageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault()
                    };
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    return new EmailResult 
                    { 
                        IsSuccess = false,
                        ErrorMessage = $"SendGrid error: {response.StatusCode} - {errorBody}"
                    };
                }
            }
            catch (OperationCanceledException)
            {
                return new EmailResult 
                { 
                    IsSuccess = false,
                    ErrorMessage = $"SendGrid request timed out after {_emailSettings.TimeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                return new EmailResult 
                { 
                    IsSuccess = false,
                    ErrorMessage = $"Exception occurred: {ex.Message}"
                };
            }
        }

        public async Task<EmailResult> SendEmailWithAttachmentsAsync(string to, string subject, string body, List<string> attachmentPaths, bool isHtml = false)
        {
            try
            {
                var client = new SendGridClient(_emailSettings.ApiKey);
                
                var from = new EmailAddress(_emailSettings.FromAddress, _emailSettings.FromName);
                var toAddress = new EmailAddress(to);
                var msg = MailHelper.CreateSingleEmail(from, toAddress, subject, isHtml ? string.Empty : body, isHtml ? body : string.Empty);

                if (attachmentPaths != null && attachmentPaths.Any())
                {
                    foreach (var attachmentPath in attachmentPaths)
                    {
                        if (File.Exists(attachmentPath))
                        {
                            var bytes = await File.ReadAllBytesAsync(attachmentPath);
                            var fileName = Path.GetFileName(attachmentPath);
                            var contentType = GetContentType(fileName);
                            var disposition = "attachment";

                            msg.AddAttachment(fileName, Convert.ToBase64String(bytes), contentType, disposition);
                        }
                    }
                }

                // Use CancellationToken for timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_emailSettings.TimeoutSeconds));
                var response = await client.SendEmailAsync(msg, cts.Token);
                
                if (response.IsSuccessStatusCode)
                {
                    return new EmailResult 
                    { 
                        IsSuccess = true,
                        MessageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault()
                    };
                }
                else
                {
                    var errorBody = await response.Body.ReadAsStringAsync();
                    return new EmailResult 
                    { 
                        IsSuccess = false,
                        ErrorMessage = $"SendGrid error: {response.StatusCode} - {errorBody}"
                    };
                }
            }
            catch (OperationCanceledException)
            {
                return new EmailResult 
                { 
                    IsSuccess = false,
                    ErrorMessage = $"SendGrid request timed out after {_emailSettings.TimeoutSeconds} seconds"
                };
            }
            catch (Exception ex)
            {
                return new EmailResult 
                { 
                    IsSuccess = false,
                    ErrorMessage = $"Exception occurred: {ex.Message}"
                };
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".txt" => "text/plain",
                ".doc" or ".docx" => "application/msword",
                ".xls" or ".xlsx" => "application/vnd.ms-excel",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
    }
}