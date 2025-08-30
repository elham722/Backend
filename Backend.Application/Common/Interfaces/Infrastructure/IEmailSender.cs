using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.Common.Interfaces.Infrastructure;

/// <summary>
/// Result of email sending operation
/// </summary>
public class EmailResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? MessageId { get; set; }
}

/// <summary>
/// Interface for email sending services
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email asynchronously
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <param name="isHtml">Whether the body is HTML</param>
    /// <returns>EmailResult with success status and error details</returns>
    Task<EmailResult> SendEmailAsync(string to, string subject, string body, bool isHtml = false);

    /// <summary>
    /// Sends an email with attachments asynchronously
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <param name="attachmentPaths">List of file paths to attach</param>
    /// <param name="isHtml">Whether the body is HTML</param>
    /// <returns>EmailResult with success status and error details</returns>
    Task<EmailResult> SendEmailWithAttachmentsAsync(string to, string subject, string body, List<string> attachmentPaths, bool isHtml = false);
}
