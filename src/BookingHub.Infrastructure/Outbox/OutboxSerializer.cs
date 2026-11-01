using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;
using BookingHub.Domain.ValueObjects;
using BookingHub.Infrastructure.Notifications;

namespace BookingHub.Infrastructure.Outbox;

/// <summary>
/// Turns outbox payloads — every domain event, <see cref="EmailMessage"/> and <see cref="SmsMessage"/> —
/// into JSON and back. Value objects have private constructors and validating factories, so each
/// one is written as plain JSON and read back through its own factory; Domain stays free of
/// serialization attributes.
/// </summary>
internal static class OutboxSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new StringValueConverter<PhoneNumber>(phone => phone.Value, value => Unwrap(PhoneNumber.Create(value))),
            new StringValueConverter<Email>(email => email.Value, value => Unwrap(Email.Create(value))),
            new StringValueConverter<SecurityToken>(token => token.Value, SecurityToken.FromExisting),
            new ShapeConverter<TimeSlot, TimeSlotJson>(
                slot => new TimeSlotJson(slot.StartUtc, slot.EndUtc),
                json => Unwrap(TimeSlot.Create(json.StartUtc, json.EndUtc))),
            new ShapeConverter<ClientContact, ClientContactJson>(
                contact => new ClientContactJson(contact.Phone, contact.Name, contact.Email),
                json => Unwrap(ClientContact.Create(json.Phone, json.Name, json.Email)))
        }
    };

    /// <summary>Keyed by the type's name, which is what <see cref="OutboxMessage.Type"/> stores.</summary>
    private static readonly FrozenDictionary<string, Type> PayloadTypes = typeof(IDomainEvent).Assembly.GetTypes()
        .Where(type => type is { IsClass: true, IsAbstract: false } && type.IsAssignableTo(typeof(IDomainEvent)))
        .Append(typeof(EmailMessage))
        .Append(typeof(SmsMessage))
        .ToFrozenDictionary(type => type.Name);

    public static string GetTypeName(object payload)
    {
        var name = payload.GetType().Name;
        return PayloadTypes.ContainsKey(name)
            ? name
            : throw new ArgumentException($"{name} cannot be stored in the outbox.", nameof(payload));
    }

    public static string Serialize(object payload) => JsonSerializer.Serialize(payload, payload.GetType(), Options);

    public static object Deserialize(string typeName, string payload)
    {
        if (!PayloadTypes.TryGetValue(typeName, out var type))
            throw new InvalidOperationException($"Unknown outbox message type '{typeName}'.");

        return JsonSerializer.Deserialize(payload, type, Options)
            ?? throw new JsonException($"The {typeName} payload is empty.");
    }

    private static T Unwrap<T>(Result<T> result) =>
        result.IsSuccess ? result.Value : throw new JsonException($"Stored {typeof(T).Name} is invalid: {result.Error.Message}");

    private sealed record TimeSlotJson(DateTime StartUtc, DateTime EndUtc);

    private sealed record ClientContactJson(PhoneNumber Phone, string? Name, Email? Email);

    /// <summary>A value object that wraps one string, written as that string.</summary>
    private sealed class StringValueConverter<T>(Func<T, string> write, Func<string, T> read) : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            read(reader.GetString() ?? throw new JsonException($"{typeof(T).Name} cannot be null."));

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteStringValue(write(value));
    }

    /// <summary>A value object written through a plain record of the same shape.</summary>
    private sealed class ShapeConverter<T, TJson>(Func<T, TJson> write, Func<TJson, T> read) : JsonConverter<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            read(JsonSerializer.Deserialize<TJson>(ref reader, options)
                ?? throw new JsonException($"{typeof(T).Name} cannot be null."));

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            JsonSerializer.Serialize(writer, write(value), options);
    }
}