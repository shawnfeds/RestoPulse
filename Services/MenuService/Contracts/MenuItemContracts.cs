namespace RestoPulse.MenuService.Contracts;

public record MenuItemResponse(int Id, string Name, string? Description, decimal Price, int CategoryId, string CategoryName, bool IsAvailable, int PreparationTime, decimal TaxRate);
public record CreateMenuItemRequest(string Name, string? Description, decimal Price, int CategoryId, int PreparationTime, decimal TaxRate);
public record UpdateMenuItemRequest(string Name, string? Description, decimal Price, int CategoryId, int PreparationTime, decimal TaxRate);