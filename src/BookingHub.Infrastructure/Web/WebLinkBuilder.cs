using BookingHub.Application.Common.Notifications;
using Microsoft.Extensions.Options;

namespace BookingHub.Infrastructure.Web;

internal sealed class WebLinkBuilder(IOptions<WebAppOptions> options) : IWebLinkBuilder
{
    public string Build(string relativePathAndQuery)
    {
        var path = relativePathAndQuery.StartsWith('/') ? relativePathAndQuery : $"/{relativePathAndQuery}";
        return $"{options.Value.BaseUrl.TrimEnd('/')}{path}";
    }
}