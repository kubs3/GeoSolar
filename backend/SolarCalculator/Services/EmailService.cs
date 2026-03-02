using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SolarCalculator.Models;
namespace SolarCalculator.Services;
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpSettings _settings;

    public EmailService(ILogger<EmailService> logger, IOptions<SmtpSettings> options)
    {
        _logger = logger;
        _settings = options.Value;
    }

    public async Task SendLeadEmailAsync(LeadRequest request)
    {
        if (string.IsNullOrEmpty(_settings.Host))
        {
            _logger.LogWarning("SMTP Host is not configured. Simulating email send for lead {Name}...", request.Name);
            return;
        }

        try
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromAddress, "GeoSolar App"),
                Subject = $"New Solar Lead: {request.Name}",
                Body = $"A new lead has been submitted!\n\nName: {request.Name}\nEmail: {request.Email}\nPhone: {request.Phone}\nRoof Area: {request.RoofArea} sqm\nOrientation: {request.Orientation}\nEst. Power: {request.EstimatedKWp} kWp",
                IsBodyHtml = false,
            };

            _logger.LogInformation("Attempting to send email FROM: {FromAddress} TO: {ToAddress}", _settings.FromAddress, _settings.FromAddress);
            mailMessage.To.Add(new MailAddress(_settings.FromAddress));
            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email successfully sent for lead {Name}.", request.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email for lead {Name}", request.Name);
        }
    }
}
