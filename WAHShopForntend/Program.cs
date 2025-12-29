using Microsoft.AspNetCore.Components.Authorization;
using WAHShopForntend.Components;
using WAHShopForntend.Components.AddressesF;
using WAHShopForntend.Components.Cart;
using WAHShopForntend.Components.Login;
using WAHShopForntend.Components.Models;
using WAHShopForntend.Components.OrderF;
using WAHShopForntend.Components.ProductsF;
using WAHShopForntend.Components.EmailF;
using WAHShopForntend.Components.CategoriesF;
using WAHShopForntend.Components.ProductGroupF;
using WAHShopForntend.Components.DiscountF;
using WAHShopForntend.Components.SearchF;
using WAHShopForntend.Components.ImagesF;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
// langauge 
builder.Services.AddLocalization();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(5); // Wartet 5 Minuten, bevor die Verbindung getrennt wird
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);    // Alle 15 Sekunden ein Ping durchführen, um die Verbindung aufrechtzuerhalten
        options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    })
    .AddCircuitOptions(options =>
    {
        // Speichert den Benutzerstatus für 10 Minuten nach Verbindungsverlust
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
        options.JSInteropDefaultCallTimeout = TimeSpan.FromSeconds(30);
    });
// project info
builder.Services.Configure<ProjectInfo>(builder.Configuration.GetSection("ProjectInfo"));
// app config
builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("AppConfig"));

// http client with base address from app config
builder.Services.AddScoped(sp =>
{
    var config = sp.GetRequiredService<IOptions<AppConfig>>().Value;
    return new HttpClient { BaseAddress = config.ApiUri };
});
// Authorization
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();

// cache service
builder.Services.AddMemoryCache();

// auth AuthService
builder.Services.AddScoped<AuthService>();
// products
builder.Services.AddScoped<ProductService>();
// addresses
builder.Services.AddScoped<AddressService>();
// cart
builder.Services.AddScoped<CartService>();
// order
builder.Services.AddScoped<OrderService>();
// email
builder.Services.AddScoped<EmailService>();
// Categories
builder.Services.AddScoped<CategoryService>();
// GroupProducts
builder.Services.AddScoped<ProductGroupService>();
// GroupProducts
builder.Services.AddScoped<DiscountService>();
// Search
builder.Services.AddScoped<SearchService>();
// ProductImages
builder.Services.AddScoped<ProductImagesService>();
// Carousel Images
builder.Services.AddScoped<CarouselImagesService>();
//

var app = builder.Build();
// language aktivation
var supportedCultures = new[] { "ar", "de" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("de")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
// lesen der Cookies für die Sprache
localizationOptions.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
/*builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7078); 
});*/

// for https
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
