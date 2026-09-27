using System.Globalization;

namespace BookingHub.Mobile.Services;

/// <summary>
/// Thin, MAUI-free wrapper around ILocalizationResourceManager (from LocalizationResourceManager.Maui) —
/// exists purely so ViewModels don't reference a MAUI-only package directly and can be compiled/tested under
/// a plain net10.0 target. The real app registers LocalizationServiceAdapter; tests substitute this.
/// </summary>
public interface ILocalizationService
{
    string this[string key] { get; }
    CultureInfo CurrentCulture { get; set; }
}