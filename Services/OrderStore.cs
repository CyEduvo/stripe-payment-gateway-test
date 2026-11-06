// Keeps classroom orders in memory; a real app would use a database.
using System.Collections.Concurrent;
using GemShop.Models;

namespace GemShop.Services;

public class OrderStore
{
    private readonly ConcurrentDictionary<string, Order> orders = new();

    public void Add(Order order) => orders[order.Id] = order;
    public Order? Find(string? id) => id != null && orders.TryGetValue(id, out var order) ? order : null;
    public ICollection<Order> All() => orders.Values;
}
