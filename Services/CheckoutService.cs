// Validates browser carts and confirms payments using Stripe's server response.
using GemShop.Models;
using Stripe.Checkout;

namespace GemShop.Services;

public class CheckoutService(OrderStore orders, IStripeGateway stripe)
{
    public string? ValidateCart(CartRequest? cart, out List<OrderLine> lines)
    {
        lines = new List<OrderLine>();
        if (cart?.Items == null || cart.Items.Count == 0) return "Your cart is empty.";
        var seen = new HashSet<string>();
        foreach (var item in cart.Items)
        {
            var product = Catalog.Find(item?.ProductId);
            if (product == null) return "Your cart contains an unknown product.";
            if (item!.Quantity < 1 || item.Quantity > 10) return "Choose between 1 and 10 of each item.";
            // Reject duplicate lines so they cannot bypass the per-item limit.
            if (!seen.Add(product.Id)) return "Each product must appear only once in the cart.";
            lines.Add(new OrderLine(product.Id, product.Name, product.Price, item.Quantity));
        }
        return null;
    }

    public async Task<string> CreateCheckout(List<OrderLine> lines, string baseUrl)
    {
        var order = new Order(lines);
        orders.Add(order);
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            ClientReferenceId = order.Id,
            Metadata = new Dictionary<string, string> { ["order_id"] = order.Id },
            SuccessUrl = baseUrl + "/checkout/success?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = baseUrl + "/checkout/cancel?order_id=" + order.Id,
            LineItems = new List<SessionLineItemOptions>()
        };
        foreach (var line in order.Lines)
        {
            options.LineItems.Add(new SessionLineItemOptions
            {
                Quantity = line.Quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "myr",
                    UnitAmount = line.UnitPrice,
                    ProductData = new SessionLineItemPriceDataProductDataOptions { Name = line.Name }
                }
            });
        }
        var session = await stripe.CreateSession(options);
        order.SetSession(session.Id);
        return session.Url;
    }

    public async Task<Order?> FulfillCheckout(string sessionId)
    {
        var session = await stripe.GetSession(sessionId);
        var order = orders.Find(session.ClientReferenceId);
        // Unrelated CLI events and orders lost after a restart are harmless.
        if (order == null || order.SessionId != session.Id) return null;
        if (!session.Livemode && session.Currency == "myr"
            && (session.PaymentStatus == "paid" || session.PaymentStatus == "no_payment_required")
            && session.AmountTotal == order.Total)
        {
            order.MarkPaid();
        }
        return order;
    }
}
