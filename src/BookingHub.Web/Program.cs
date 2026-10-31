using BookingHub.Web.Api;
using BookingHub.Web.Auth;
using BookingHub.Web.Components;
using BookingHub.Web.Localization;
using BookingHub.Web.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddLocalization();

builder.Services.TryAddSingleton(TimeProvider.System);
builder.Services.AddStaffAuthentication();

var apiBaseUrl = ReadApiBaseUrl(builder.Configuration);

builder.Services.AddHttpClient<IPublicCatalogApiClient, PublicCatalogApiClient>(client =>
    client.BaseAddress = apiBaseUrl);
builder.Services.AddHttpClient<IPublicBookingsApiClient, PublicBookingsApiClient>(client =>
    client.BaseAddress = apiBaseUrl);
builder.Services.AddHttpClient<IPublicReviewsApiClient, PublicReviewsApiClient>(client =>
    client.BaseAddress = apiBaseUrl);
builder.Services.AddHttpClient<IPublicWaitlistApiClient, PublicWaitlistApiClient>(client =>
    client.BaseAddress = apiBaseUrl);
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
    client.BaseAddress = apiBaseUrl);

builder.Services.AddHttpClient(AuthorizedApiClient.HttpClientName, client =>
    client.BaseAddress = apiBaseUrl);
builder.Services.AddScoped<AuthorizedApiClient>();
builder.Services.AddScoped<IMeApiClient, MeApiClient>();
builder.Services.AddScoped<IOrganizationsApiClient, OrganizationsApiClient>();
builder.Services.AddScoped<ILocationsApiClient, LocationsApiClient>();
builder.Services.AddScoped<IServicesApiClient, ServicesApiClient>();
builder.Services.AddScoped<IEmployeesApiClient, EmployeesApiClient>();
builder.Services.AddScoped<ISchedulesApiClient, SchedulesApiClient>();
builder.Services.AddScoped<IStaffBookingsApiClient, StaffBookingsApiClient>();
builder.Services.AddScoped<IClientsApiClient, ClientsApiClient>();
builder.Services.AddScoped<IAnalyticsApiClient, AnalyticsApiClient>();
builder.Services.AddScoped<IStaffReviewsApiClient, StaffReviewsApiClient>();
builder.Services.AddScoped<IMembersApiClient, MembersApiClient>();
builder.Services.AddScoped<MembershipCache>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();

app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture(SupportedCultures.Default)
    .AddSupportedCultures(SupportedCultures.All)
    .AddSupportedUICultures(SupportedCultures.All));

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapCultureSwitch();
app.MapStaffLogout();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Fails at startup with an actionable message instead of on the first API call.
static Uri ReadApiBaseUrl(IConfiguration configuration) =>
    Uri.TryCreate(configuration["Api:BaseUrl"], UriKind.Absolute, out var url)
        ? url
        : throw new InvalidOperationException(
            "Api:BaseUrl must be the absolute URL of BookingHub.API, e.g. https://localhost:7117 " +
            "(appsettings.Development.json locally, the Api__BaseUrl environment variable when deployed).");