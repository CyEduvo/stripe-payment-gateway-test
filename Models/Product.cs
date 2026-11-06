// Defines the trusted catalog; money is stored as whole sen, never browser prices.
namespace GemShop.Models;

public record Product(string Id, string Name, long Price, string Description);

public static class Catalog
{
    public static IReadOnlyList<Product> Products { get; } = Array.AsReadOnly(new[]
    {
        new Product("gems-100", "100 Gems", 490, "A little spark for your next adventure."),
        new Product("gems-550", "550 Gems", 1990, "Stock up for your next summon."),
        new Product("gems-1200", "1,200 Gems", 3990, "A treasure chest of possibilities."),
        new Product("skin-ember", "Ember Queen Skin", 990, "Give your hero a fiery new look."),
        new Product("pass-monthly", "Monthly Pass", 1490, "One month of adventure. One-time purchase.")
    });

    public static Product? Find(string? id)
    {
        foreach (var product in Products)
        {
            if (product.Id == id) return product;
        }
        return null;
    }
}
