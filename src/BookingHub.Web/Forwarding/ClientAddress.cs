namespace BookingHub.Web.Forwarding;

/// <summary>
/// The browser's IP address inside an interactive circuit, which has no HttpContext to read it
/// from. Routes stores it once, from the HTTP request that rendered the page.
/// </summary>
internal sealed class ClientAddress
{
    public string? Value { get; set; }
}