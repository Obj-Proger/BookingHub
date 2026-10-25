using BookingHub.Web.Api;
using BookingHub.Web.Auth;
using BookingHub.Web.Components;
using BookingHub.Web.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.TryAddSingleton(TimeProvider.System);
builder.Services.AddStaffAuthentication();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Api:BaseUrl is not configured.");

builder.Services.AddHttpClient<IPublicCatalogApiClient, PublicCatalogApiClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IPublicBookingsApiClient, PublicBookingsApiClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IPublicReviewsApiClient, PublicReviewsApiClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IPublicWaitlistApiClient, PublicWaitlistApiClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
    client.BaseAddress = new Uri(apiBaseUrl));

builder.Services.AddHttpClient(AuthorizedApiClient.HttpClientName, client =>
    client.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddScoped<AuthorizedApiClient>();
builder.Services.AddScoped<IMeApiClient, MeApiClient>();
builder.Services.AddScoped<IOrganizationsApiClient, OrganizationsApiClient>();
builder.Services.AddScoped<ILocationsApiClient, LocationsApiClient>();
builder.Services.AddScoped<IServicesApiClient, ServicesApiClient>();
builder.Services.AddScoped<MembershipCache>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaffLogout();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();