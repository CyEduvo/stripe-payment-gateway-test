// Accepts only product IDs and quantities; extra browser fields are ignored.
namespace GemShop.Models;

public class CartRequest
{
    public List<CartItem?>? Items { get; set; }
}

public class CartItem
{
    public string? ProductId { get; set; }
    public int Quantity { get; set; }
}
