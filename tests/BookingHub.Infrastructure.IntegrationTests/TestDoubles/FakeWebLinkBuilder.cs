using BookingHub.Application.Common.Notifications;

namespace BookingHub.Infrastructure.IntegrationTests.TestDoubles;

internal sealed class FakeWebLinkBuilder : IWebLinkBuilder
{
    public string Build(string relativePathAndQuery) => $"https://example.test{relativePathAndQuery}";
}