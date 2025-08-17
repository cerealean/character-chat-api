namespace CharacterChatApi.Models;

public class Message
{
    public required string Id { get; set; } // Ulid string
    public required string ChatId { get; set; } // FK to Chat
    public MessageRole Role { get; set; }
    public required string Content { get; set; }
    public int? Tokens { get; set; }
    public bool Flagged { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Chat Chat { get; set; } = null!;
}