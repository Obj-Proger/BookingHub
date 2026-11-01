namespace BookingHub.Application.Common.Security;

/// <summary>
/// Marks a request anyone may send without signing in — the guest-facing booking flow. Where such
/// a request touches a specific booking or waitlist entry, it carries that record's token and the
/// handler verifies it.
/// </summary>
public interface IPublicRequest;