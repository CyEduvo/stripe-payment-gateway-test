// Uses the official Stripe client and refuses all non-test secret keys.
using Stripe;
using Stripe.Checkout;

namespace GemShop.Services;

public class StripeGateway(IConfiguration configuration) : IStripeGateway
{
    private string Key => configuration["Stripe:SecretKey"]?.Trim() ?? "";

    public string? SetupError
    {
        get
        {
            if (Key.StartsWith("sk_live_", StringComparison.Ordinal))
                return "This demo only works with test keys. Live keys are refused.";
            if (!Key.StartsWith("sk_test_", StringComparison.Ordinal) || Key == "sk_test_...")
                return "Set up Stripe: add your sk_test_ secret key with dotnet user-secrets. See the README.";
            return null;
        }
    }

    private StripeClient Client()
    {
        // Check at the network boundary too, so no caller can bypass test mode.
        if (SetupError != null) throw new InvalidOperationException(SetupError);
        return new StripeClient(Key);
    }

    public Task<Session> CreateSession(SessionCreateOptions options) => Client().V1.Checkout.Sessions.CreateAsync(options);
    public Task<Session> GetSession(string sessionId) => Client().V1.Checkout.Sessions.GetAsync(sessionId);
}
