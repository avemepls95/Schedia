namespace Avemepls.Infrastructure.Email;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string email, string username, string confirmationLink, CancellationToken ct = default);

    Task SendPasswordResetAsync(string email, string username, string resetLink, CancellationToken ct = default);

    Task SendWelcomeEmailAsync(string email, string username, CancellationToken ct = default);
}