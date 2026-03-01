using SolarCalculator.Models;
namespace SolarCalculator.Services;
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    public EmailService(ILogger<EmailService> logger) { _logger = logger; }
    public Task SendLeadEmailAsync(LeadRequest request)
    {
        _logger.LogInformation("Simulating email send for lead {Name}...", request.Name);
        return Task.CompletedTask;
    }
}
