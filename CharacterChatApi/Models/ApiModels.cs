namespace CharacterChatApi.Models;

public record CreateUserRequest(string Email, string Name);

public record UserResponse(int Id, string Email, string Name, string Role, DateTime CreatedAt);

public record HealthResponse(string Status);

// Character DTOs
public record CreateCharacterRequest(
    string Slug,
    string Title,
    string Summary,
    string? AvatarUrl,
    string SystemPrompt,
    string? AuthorNote,
    string? StyleGuidance,
    string? ExampleDialog,
    string[] Tags,
    Visibility? Visibility,
    string? ModelConfigJson);

public record CharacterResponse(
    string Id,
    string Slug,
    string Title,
    string Summary,
    string? AvatarUrl,
    string SystemPrompt,
    string? AuthorNote,
    string? StyleGuidance,
    string? ExampleDialog,
    string[] Tags,
    Visibility Visibility,
    string ModelConfigJson,
    DateTime CreatedAt,
    DateTime UpdatedAt);

// Chat DTOs
public record CreateChatRequest(string CharacterId, int? UserId);

public record ChatResponse(
    string Id,
    string CharacterId,
    int UserId,
    string? Title,
    string? MemoryStateJson,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    MessageResponse[] Messages);

public record MessageResponse(
    string Id,
    string ChatId,
    MessageRole Role,
    string Content,
    int? Tokens,
    bool Flagged,
    DateTime CreatedAt);