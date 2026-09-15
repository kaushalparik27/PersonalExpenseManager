using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PersonalExpenseManager.Application.Common;

namespace PersonalExpenseManager.Infrastructure.Email;

// Development-only IEmailSender: logs the email instead of sending it, so
// password-reset links are visible in the backend console. Swap in a real
// provider (e.g. SendGrid, SMTP) via DI for production without touching callers.
//
// The full body (which can carry a password-reset link/token) is only ever logged
// in Development — logging a reset secret to a non-development log stream would
// violate the "never log passwords/tokens/reset links" rule the moment this ran
// unmodified in a real environment. Outside Development, this still exists as a
// safe no-op so misconfiguration is loud (a warning) instead of a silent secret leak.
public class DevEmailSender(ILogger<DevEmailSender> logger, IHostEnvironment environment) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (environment.IsDevelopment())
        {
            logger.LogInformation(
                "[DEV EMAIL] To: {ToEmail} | Subject: {Subject}\n{Body}",
                toEmail,
                subject,
                htmlBody);
        }
        else
        {
            logger.LogWarning(
                "No real IEmailSender is configured for this environment — an email to {ToEmail} ({Subject}) was not sent.",
                toEmail,
                subject);
        }

        return Task.CompletedTask;
    }
}
