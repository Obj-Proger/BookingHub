using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using BookingHub.Web.Resources;

namespace BookingHub.Web.Localization;

/// <summary>A required text field whose error message is a key in the shared resources.</summary>
public sealed class LocalizedRequiredAttribute(string messageKey) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
        value is string text && !string.IsNullOrWhiteSpace(text)
            ? ValidationResult.Success
            : LocalizedValidation.Failure(messageKey, validationContext);
}

/// <summary>An email address field whose error message is a key in the shared resources; an empty value is left to a required rule.</summary>
public sealed class LocalizedEmailAddressAttribute(string messageKey) : ValidationAttribute
{
    private static readonly EmailAddressAttribute Format = new();

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext) =>
        value is null or "" || Format.IsValid(value)
            ? ValidationResult.Success
            : LocalizedValidation.Failure(messageKey, validationContext);
}

internal static class LocalizedValidation
{
    public static ValidationResult Failure(string messageKey, ValidationContext context)
    {
        var localizer = (IStringLocalizer<SharedResource>?)context.GetService(typeof(IStringLocalizer<SharedResource>));
        var message = localizer?[messageKey].Value ?? messageKey;

        return context.MemberName is { } member
            ? new ValidationResult(message, [member])
            : new ValidationResult(message);
    }
}