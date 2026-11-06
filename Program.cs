// Registers MVC and the three small services used by the shop.
using GemShop.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
builder.Services.AddSingleton<OrderStore>();
builder.Services.AddSingleton<IStripeGateway, StripeGateway>();
builder.Services.AddSingleton<CheckoutService>();

var app = builder.Build();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.Run();

// Allows the test project to start the real MVC application in memory.
public partial class Program { }
