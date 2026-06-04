namespace RestoPulse.UserService.Contracts;

public record LoginRequest(
    string Username,
    string Password);

public record CreateUserRequest(
    string Username,
    string FullName,
    string Password,
    string Role);

public record LoginResponse(
    string Token,
    string Username,
    string FullName,
    string Role);

public record UserResponse(
    int Id,
    string Username,
    string FullName,
    string Role,
    DateTime CreatedAt);
