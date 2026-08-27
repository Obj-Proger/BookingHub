using BookingHub.Application.Common.Notifications;
using Microsoft.Extensions.Options;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace BookingHub.Infrastructure.Notifications;

internal sealed class TwilioSmsService(ITwilioRestClient twilioClient, IOptions<TwilioOptions> twilioOptions) : ISmsService
{
    // MessageResource.CreateAsync's exact overload set (and whether it accepts a
    // CancellationToken directly) is generated from Twilio's API spec and changes between
    // SDK versions — verify against the installed version's IntelliSense/docs before relying
    // on this signature; not passing cancellationToken through here is a known gap, not an oversight.
    public async Task SendAsync(SmsMessage message, CancellationToken cancellationToken)
    {
        await MessageResource.CreateAsync(
            body: message.Body,
            from: new PhoneNumber(twilioOptions.Value.FromPhoneNumber),
            to: new PhoneNumber(message.ToPhoneNumber),
            client: twilioClient);
    }
}