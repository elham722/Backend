using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.Common.Interfaces.Infrastructure;

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
    /// <returns>True if email was sent successfully</returns>
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false);

    /// <summary>
    /// Sends an email with attachments asynchronously
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <param name="attachmentPaths">List of file paths to attach</param>
    /// <param name="isHtml">Whether the body is HTML</param>
    /// <returns>True if email was sent successfully</returns>
    Task<bool> SendEmailWithAttachmentsAsync(string to, string subject, string body, List<string> attachmentPaths, bool isHtml = false);
}
