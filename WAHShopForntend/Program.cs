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
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);
// langauge 
builder.Services.AddLocalization();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
// project info
builder.Services.Configure<ProjectInfo>(
    builder.Configuration.GetSection("ProjectInfo"));
// api services
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7250") });
// Authorization
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();



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

var app = builder.Build();
// language aktivation
var supportedCultures = new[] { "ar", "de" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("de")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
// المهم: الكوكي أول مزود للثقافة
localizationOptions.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
