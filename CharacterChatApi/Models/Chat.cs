namespace CharacterChatApi.Models;

public class Chat
{
    public required string Id { get; set; } // Ulid string
    public required string CharacterId { get; set; } // FK to Character
    public int UserId { get; set; } // FK to User
    public string? Title { get; set; }
    public string? MemoryStateJson { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public Character Character { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = [];
}