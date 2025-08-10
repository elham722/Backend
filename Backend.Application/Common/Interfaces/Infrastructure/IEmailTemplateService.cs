using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Application.Common.Interfaces.Infrastructure
{
    /// <summary>
    /// Interface for email template service
    /// </summary>
    public interface IEmailTemplateService
    {
        /// <summary>
        /// Gets an email template and replaces placeholders with actual values
        /// </summary>
        /// <param name="templateName">Name of the template file (without extension)</param>
        /// <param name="replacements">Dictionary of placeholder-value pairs</param>
        /// <returns>Processed template content</returns>
        Task<string> GetProcessedTemplateAsync(string templateName, Dictionary<string, string> replacements);
        
        /// <summary>
        /// Gets the raw content of an email template
        /// </summary>
        /// <param name="templateName">Name of the template file (without extension)</param>
        /// <returns>Raw template content</returns>
        Task<string> GetTemplateContentAsync(string templateName);
        
        /// <summary>
        /// Clears the template cache
        /// </summary>
        void ClearCache();
    }
}