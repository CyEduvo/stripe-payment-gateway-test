// Shows the server catalog and a friendly Stripe setup message.
using GemShop.Models;
using GemShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace GemShop.Controllers;

public class ShopController(IStripeGateway stripe) : Controller
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        ViewBag.SetupError = stripe.SetupError;
        return View(Catalog.Products);
    }
}
