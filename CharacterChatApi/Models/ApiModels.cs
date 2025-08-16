namespace CharacterChatApi.Models;

public record CreateUserRequest(string Email, string Name);

public record UserResponse(int Id, string Email, string Name, string Role, DateTime CreatedAt);

public record HealthResponse(string Status);