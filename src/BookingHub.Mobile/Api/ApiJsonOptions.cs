using System.Text.Json;
using System.Text.Json.Serialization;

namespace BookingHub.Mobile.Api;

/// <summary>
/// Shared by every API client — the server serializes enums as strings (JsonStringEnumConverter)
/// which System.Text.Json does not assume by default (it expects numbers unless
/// told otherwise). Every request/response touching an enum-bearing DTO must use this, or
/// deserialization fails silently against a perfectly valid server response.
/// </summary>
internal static class ApiJsonOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}