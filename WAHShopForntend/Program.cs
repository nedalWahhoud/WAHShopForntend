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
using Microsoft.Extensions.Options;
using WAHShopForntend.Components.CookieF;
using WAHShopForntend.Components.CustomersF;
using WAHShopForntend.Components.TransactionsCustomersF;
using WAHShopForntend.Components.DebtF;

var builder = WebApplication.CreateBuilder(args);

/* local test port listen */
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(7078, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

// langauge 
builder.Services.AddLocalization();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(10); // Wartet 10 Minuten, bevor die Verbindung getrennt wird
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
// Developer Info 
builder.Services.Configure<DeveloperInfo>(builder.Configuration.GetSection("DeveloperInfo"));
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
// cookie service
builder.Services.AddScoped<CookieService>();
// Customer service
builder.Services.AddScoped<CustomersService>();
// TransactionsCustomers service
builder.Services.AddScoped<TransactionsCustomersService>();
// DebtCustomers service
builder.Services.AddScoped<DebtService>();

//
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// for https
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
