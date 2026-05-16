namespace RestoPulse.MenuService.Domain.Entities;

public class MenuItem
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public bool IsAvailable { get; private set; }
    public int PreparationTime { get; private set; } // minutes
    public decimal TaxRate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private MenuItem() { }

    public static MenuItem Create(
        string name, string? description, decimal price,
        int categoryId, int preparationTime, decimal taxRate)
    {
        return new MenuItem
        {
            Name = name,
            Description = description,
            Price = price,
            CategoryId = categoryId,
            PreparationTime = preparationTime,
            TaxRate = taxRate,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description, decimal price,
        int categoryId, int preparationTime, decimal taxRate)
    {
        Name = name;
        Description = description;
        Price = price;
        CategoryId = categoryId;
        PreparationTime = preparationTime;
        TaxRate = taxRate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleAvailability() => IsAvailable = !IsAvailable;
}