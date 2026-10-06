namespace BookingHub.Application.Common.Notifications;

/// <summary>Turns a relative path (e.g. "/book/acme/confirm/{id}?token=...") into an absolute,
/// clickable URL pointing at the Web app — notification messages can't send relative links.</summary>
public interface IWebLinkBuilder
{
    string Build(string relativePathAndQuery);
}