// Verifies signed Stripe events before asking Stripe to confirm the payment.
using GemShop.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using CheckoutService = GemShop.Services.CheckoutService;

namespace GemShop.Controllers;

public class WebhookController(IConfiguration configuration, CheckoutService checkout) : Controller
{
    [HttpPost("/webhook")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Index()
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        var secret = configuration["Stripe:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(secret) || !secret.StartsWith("whsec_", StringComparison.Ordinal))
            return BadRequest(new { error = "Configure the Stripe webhook signing secret." });
        Event stripeEvent;
        try
        {
            // CLI versions may differ. Signature verification stays enabled, but
            // disabling the version check means teachers must test SDK upgrades.
            stripeEvent = EventUtility.ConstructEvent(body, Request.Headers["Stripe-Signature"].ToString(),
                secret, throwOnApiVersionMismatch: false);
        }
        catch (StripeException) { return BadRequest(); }
        catch (ArgumentException) { return BadRequest(); }
        catch (Newtonsoft.Json.JsonException) { return BadRequest(); }

        if (stripeEvent.Type == "checkout.session.completed" || stripeEvent.Type == "checkout.session.async_payment_succeeded")
        {
            if (stripeEvent.Data.Object is not Session session) return BadRequest();
            try { await checkout.FulfillCheckout(session.Id); }
            // A failure must remain non-200 so Stripe knows to retry delivery.
            catch (StripeException) { return StatusCode(503); }
            catch (HttpRequestException) { return StatusCode(503); }
            catch (InvalidOperationException) { return StatusCode(503); }
        }
        return Ok();
    }
}
