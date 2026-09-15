using Microsoft.Extensions.Logging;
using PersonalExpenseManager.Application.Common;

namespace PersonalExpenseManager.Infrastructure.Email;

// Development-only IEmailSender: logs the email instead of sending it, so
// password-reset links are visible in the backend console. Swap in a real
// provider (e.g. SendGrid, SMTP) via DI for production without touching callers.
public class DevEmailSender(ILogger<DevEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[DEV EMAIL] To: {ToEmail} | Subject: {Subject}\n{Body}",
            toEmail,
            subject,
            htmlBody);

        return Task.CompletedTask;
    }
}
