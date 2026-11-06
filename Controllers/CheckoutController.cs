// Starts hosted Checkout and displays payment status checked by the server.
using GemShop.Models;
using GemShop.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using CheckoutService = GemShop.Services.CheckoutService;

namespace GemShop.Controllers;

public class CheckoutController(CheckoutService checkout, IStripeGateway stripe) : Controller
{
    [HttpPost("/checkout/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] CartRequest? cart)
    {
        if (!ModelState.IsValid) return BadRequest(new { error = "Send a valid cart with whole-number quantities." });
        var error = checkout.ValidateCart(cart, out var lines);
        if (error != null) return BadRequest(new { error });
        if (stripe.SetupError != null) return BadRequest(new { error = stripe.SetupError });
        try
        {
            // Fixed local origin avoids trusting a browser-supplied return URL or Host.
            var url = await checkout.CreateCheckout(lines, "http://localhost:5000");
            return Json(new { url });
        }
        catch (StripeException)
        {
            return StatusCode(502, new { error = "Stripe could not start checkout. Check your test key and connection, then try again." });
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new { error = "Could not reach Stripe. Please try again." });
        }
    }

    [HttpGet("/checkout/success")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Success(string? session_id)
    {
        Order? order = null;
        if (!string.IsNullOrWhiteSpace(session_id) && session_id.StartsWith("cs_test_", StringComparison.Ordinal)
            && stripe.SetupError == null)
        {
            try { order = await checkout.FulfillCheckout(session_id); }
            catch (StripeException) { ViewBag.LookupError = "We could not find this payment. Check the session ID or try again shortly."; }
            catch (HttpRequestException) { ViewBag.LookupError = "Stripe is temporarily unavailable. Please try again shortly."; }
        }
        return View(order);
    }

    [HttpGet("/checkout/cancel")]
    public IActionResult Cancel() => View();
}
