using System.Reflection;
using BookingHub.Application.Common.Messaging;

namespace BookingHub.Application.Common.Security;

/// <summary>
/// Startup check that every request states who may send it, so a forgotten marker stops the
/// application from starting instead of leaving an operation open to anyone.
/// </summary>
internal static class RequestAccessValidation
{
    private static readonly Type[] CallerKinds = [typeof(IPublicRequest), typeof(IAuthenticatedRequest), typeof(ISystemRequest)];

    private static readonly Type[] OrganizationRoleMarkers =
        [typeof(IRequireBookingAccess), typeof(IRequireLocationManagement), typeof(IRequireOrganizationManagement)];

    /// <exception cref="InvalidOperationException">
    /// A request in <paramref name="assembly"/> declares no access marker, or more than one.
    /// </exception>
    public static void EnsureEveryRequestDeclaresAccess(Assembly assembly)
    {
        var invalid = assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false } && type.GetInterfaces().Any(IsRequestInterface))
            .Where(type => !DeclaresExactlyOneAccessRule(type))
            .Select(type => type.FullName)
            .OrderBy(name => name)
            .ToList();

        if (invalid.Count > 0)
        {
            throw new InvalidOperationException(
                "Every request must declare exactly one access marker: IPublicRequest, IAuthenticatedRequest, " +
                "ISystemRequest, or a single organization marker (IRequireOrganizationMembership or one of the " +
                $"narrower IRequire* markers). Offending requests: {string.Join(", ", invalid)}.");
        }
    }

    private static bool IsRequestInterface(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IRequest<>);

    private static bool DeclaresExactlyOneAccessRule(Type requestType)
    {
        var interfaces = requestType.GetInterfaces().ToHashSet();

        // The narrower organization markers all extend IRequireOrganizationMembership; at most one may be used.
        var organizationRoleMarkers = OrganizationRoleMarkers.Count(interfaces.Contains);
        if (organizationRoleMarkers > 1)
            return false;

        var organizationScoped = interfaces.Contains(typeof(IRequireOrganizationMembership)) ? 1 : 0;
        return CallerKinds.Count(interfaces.Contains) + organizationScoped == 1;
    }
}