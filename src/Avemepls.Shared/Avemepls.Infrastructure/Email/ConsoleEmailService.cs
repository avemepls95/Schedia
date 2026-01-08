using Microsoft.Extensions.Logging;

namespace Avemepls.Infrastructure.Email;

public class ConsoleEmailService(ILogger<ConsoleEmailService> logger) : IEmailService
{
    public Task SendEmailConfirmationAsync(string email, string username, string confirmationLink, CancellationToken ct = default)
    {
        logger.LogInformation(
            "\n========== EMAIL: Email Confirmation ==========\n" +
            "To: {Email}\n" +
            "Subject: Confirm your email address\n" +
            "Body:\n" +
            "Hi {Username},\n\n" +
            "Please confirm your email by clicking: {Link}\n" +
            "==============================================\n",
            email,
            username,
            confirmationLink);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string username, string resetLink, CancellationToken ct = default)
    {
        logger.LogInformation(
            "\n========== EMAIL: Password Reset ==========\n" +
            "To: {Email}\n" +
            "Subject: Reset your password\n" +
            "Body:\n" +
            "Hi {Username},\n\n" +
            "You requested to reset your password. Click here: {Link}\n" +
            "This link will expire in 1 hour.\n" +
            "If you didn't request this, please ignore this email.\n" +
            "==============================================\n",
            email,
            username,
            resetLink);

        return Task.CompletedTask;
    }

    public Task SendWelcomeEmailAsync(string email, string username, CancellationToken ct = default)
    {
        logger.LogInformation(
            "\n========== EMAIL: Welcome ==========\n" +
            "To: {Email}\n" +
            "Subject: Welcome to Schedia!\n" +
            "Body:\n" +
            "Hi {Username},\n\n" +
            "Welcome to Schedia! Your account has been created successfully.\n" +
            "==============================================\n",
            email,
            username);

        return Task.CompletedTask;
    }
}