namespace CharacterChatApi.Models;

public class Character
{
    public required string Id { get; set; } // Ulid string
    public required string Slug { get; set; } // unique
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public string? AvatarUrl { get; set; }
    public required string SystemPrompt { get; set; }
    public string? AuthorNote { get; set; }
    public string? StyleGuidance { get; set; }
    public string? ExampleDialog { get; set; }
    public string[] Tags { get; set; } = [];
    public Visibility Visibility { get; set; } = Visibility.PUBLIC;
    public string ModelConfigJson { get; set; } = """{"provider":"openai","model":"gpt-4o-mini","temperature":0.8}""";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Chat> Chats { get; set; } = [];
}