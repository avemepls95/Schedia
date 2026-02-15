using System.Security.Cryptography;

using Avemepls.Auth.Abstractions.Models;
using Avemepls.Core.DataAccess.Extensions;
using Avemepls.Core.Models;
using Avemepls.Domain.Exceptions;
using Avemepls.Identity.Abstraction;
using Avemepls.Identity.DataAccess;
using Avemepls.Identity.DataAccess.Models;
using Avemepls.Infrastructure.DateTime;
using Avemepls.Infrastructure.Email;
using MassTransit;

namespace Avemepls.Identity.Application.NotificationHandlers;

public class SendEmailConfirmationWhenUserRegistered(
    IdentityDbContext dbContext,
    IEmailService emailService,
    ICurrentDateTimeProvider currentDateTimeProvider,
    AppOptions appOptions,
    IdentityOptions identityOptions)
    : IConsumer<UserRegisteredViaPasswordNotification>
{
    public async Task Consume(ConsumeContext<UserRegisteredViaPasswordNotification> context)
    {
        var confirmationToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var confirmationLink = $"{appOptions.BaseUrl}/confirm-email?token={Uri.EscapeDataString(confirmationToken)}";

        await dbContext.ConfirmEmailRecords.AddAsync(new ConfirmEmailRecord
        {
            UserId = context.Message.UserId,
            EmailConfirmationToken = confirmationToken,
            EmailConfirmationTokenExpiry = currentDateTimeProvider.UtcNow.AddHours(identityOptions.EmailConfirmationTokenLifeTimeInHours)
        });

        var user = await dbContext.Users.Available().GetFirstValue(
            x => x.Id == context.Message.UserId,
            x => new
            {
                x.Username,
                x.Email,
            },
            context.CancellationToken);

        if (user is null)
        {
            throw new ObjectNotFoundException(typeof(User), context.Message.UserId);
        }

        await emailService.SendEmailConfirmationAsync(user.Email!, user.Username, confirmationLink, context.CancellationToken);

        await dbContext.SaveChangesAsync();
    }
}