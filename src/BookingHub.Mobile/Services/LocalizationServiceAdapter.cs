using System.Globalization;
using LocalizationResourceManager.Maui;

namespace BookingHub.Mobile.Services;

internal sealed class LocalizationServiceAdapter(ILocalizationResourceManager localization) : ILocalizationService
{
    public string this[string key] => localization[key];

    public CultureInfo CurrentCulture
    {
        get => localization.CurrentCulture;
        set => localization.CurrentCulture = value;
    }
}