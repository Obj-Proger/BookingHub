using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookingHub.Web.Api;

/// <summary>
/// Shared by every API client. The server serializes enums as strings (JsonStringEnumConverter),
/// while System.Text.Json expects numbers unless told otherwise, so every request and response
/// touching an enum-bearing DTO must go through these options.
/// </summary>
internal static class ApiJsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}