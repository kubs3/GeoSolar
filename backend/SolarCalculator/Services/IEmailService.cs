using SolarCalculator.Models;
namespace SolarCalculator.Services;
public interface IEmailService
{
    Task SendLeadEmailAsync(LeadRequest request);
}
