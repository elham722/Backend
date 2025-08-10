using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces.Infrastructure;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Email
{
    /// <summary>
    /// Service for managing email templates
    /// </summary>
    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly EmailSettings _emailSettings;
        private readonly Dictionary<string, string> _cachedTemplates = new Dictionary<string, string>();
        private readonly string _templatesBasePath;

        public EmailTemplateService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
            _templatesBasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates");
        }

        /// <summary>
        /// Gets an email template and replaces placeholders with actual values
        /// </summary>
        /// <param name="templateName">Name of the template file (without extension)</param>
        /// <param name="replacements">Dictionary of placeholder-value pairs</param>
        /// <returns>Processed template content</returns>
        public async Task<string> GetProcessedTemplateAsync(string templateName, Dictionary<string, string> replacements)
        {
            string templateContent = await GetTemplateContentAsync(templateName);
            return ProcessTemplate(templateContent, replacements);
        }

        /// <summary>
        /// Gets the raw content of an email template
        /// </summary>
        /// <param name="templateName">Name of the template file (without extension)</param>
        /// <returns>Raw template content</returns>
        public async Task<string> GetTemplateContentAsync(string templateName)
        {
            // Check if template is already cached
            if (_cachedTemplates.TryGetValue(templateName, out string cachedTemplate))
            {
                return cachedTemplate;
            }

            // Try to load template from file
            string templatePath = Path.Combine(_templatesBasePath, $"{templateName}.html");
            
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template '{templateName}' not found at path: {templatePath}");
            }

            string templateContent = await File.ReadAllTextAsync(templatePath);
            
            // Cache the template
            _cachedTemplates[templateName] = templateContent;
            
            return templateContent;
        }

        /// <summary>
        /// Processes a template by replacing placeholders with actual values
        /// </summary>
        /// <param name="templateContent">Raw template content</param>
        /// <param name="replacements">Dictionary of placeholder-value pairs</param>
        /// <returns>Processed template content</returns>
        private string ProcessTemplate(string templateContent, Dictionary<string, string> replacements)
        {
            if (replacements == null || !replacements.Any())
            {
                return templateContent;
            }

            string processedContent = templateContent;
            
            // Replace all placeholders with their values
            foreach (var replacement in replacements)
            {
                processedContent = processedContent.Replace($"{{{{{replacement.Key}}}}}", replacement.Value);
            }

            // Add common replacements like company name, current year, etc.
            processedContent = processedContent.Replace("{{CompanyName}}", _emailSettings.FromName);
            processedContent = processedContent.Replace("{{CurrentYear}}", DateTime.Now.Year.ToString());
            
            return processedContent;
        }

        /// <summary>
        /// Clears the template cache
        /// </summary>
        public void ClearCache()
        {
            _cachedTemplates.Clear();
        }
    }
}