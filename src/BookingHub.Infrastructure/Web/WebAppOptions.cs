namespace BookingHub.Infrastructure.Web;

public sealed class WebAppOptions
{
    public const string SectionName = "WebApp";
    public required string BaseUrl { get; init; }
}