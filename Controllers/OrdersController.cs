// Displays the orders held by this running classroom demo.
using GemShop.Services;
using Microsoft.AspNetCore.Mvc;

namespace GemShop.Controllers;

public class OrdersController(OrderStore orders) : Controller
{
    [HttpGet("/orders")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Index() => View(orders.All());
}
