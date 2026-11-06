// Keeps the two Stripe network calls replaceable in unit tests.
using Stripe.Checkout;

namespace GemShop.Services;

public interface IStripeGateway
{
    string? SetupError { get; }
    Task<Session> CreateSession(SessionCreateOptions options);
    Task<Session> GetSession(string sessionId);
}
