// Holds server-priced lines and protects payment changes from simultaneous calls.
namespace GemShop.Models;

public record OrderLine(string ProductId, string Name, long UnitPrice, int Quantity)
{
    public long Total => UnitPrice * Quantity;
}

public class Order
{
    private readonly object paymentLock = new();
    private string status = "Pending";
    private string? sessionId;

    public string Id { get; } = Guid.NewGuid().ToString("N");
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<OrderLine> Lines { get; }
    public long Total => Lines.Sum(line => line.Total);
    public string Status { get { lock (paymentLock) return status; } }
    public string? SessionId { get { lock (paymentLock) return sessionId; } }

    public Order(List<OrderLine> lines) => Lines = new List<OrderLine>(lines).AsReadOnly();

    public void SetSession(string id)
    {
        lock (paymentLock) sessionId = id;
    }

    public bool MarkPaid()
    {
        lock (paymentLock)
        {
            // Both the webhook and success page can arrive at the same time.
            if (status == "Paid") return false;
            status = "Paid";
            return true;
        }
    }
}
