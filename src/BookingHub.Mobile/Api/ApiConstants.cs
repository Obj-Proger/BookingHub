namespace BookingHub.Mobile.Api;

internal static class ApiConstants
{
    /// <summary>
    /// Android emulator can't resolve "localhost" as the host machine — 10.0.2.2 is the
    /// documented emulator alias for it. iOS simulator and Windows both reach the host directly
    /// via localhost. Update this once a real deployment exists; for now it only targets local
    /// development against dotnet run.
    /// </summary>
#if ANDROID
    public const string BaseUrl = "https://10.0.2.2:5001/";
#else
    public const string BaseUrl = "https://localhost:5001/";
#endif
}