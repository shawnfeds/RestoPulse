namespace RestoPulse.MenuService.Contracts;

public record CategoryResponse(int Id, string Name, string? Description, int DisplayOrder, bool IsActive, int ItemCount);
public record CreateCategoryRequest(string Name, string? Description, int DisplayOrder);
public record UpdateCategoryRequest(string Name, string? Description, int DisplayOrder);