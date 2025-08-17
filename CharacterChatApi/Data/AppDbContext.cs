using Microsoft.EntityFrameworkCore;
using CharacterChatApi.Models;

namespace CharacterChatApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure Character entity
        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().HasMaxLength(26); // Ulid is 26 chars
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Summary).IsRequired();
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.SystemPrompt).IsRequired();
            entity.Property(e => e.AuthorNote);
            entity.Property(e => e.StyleGuidance);
            entity.Property(e => e.ExampleDialog);
            entity.Property(e => e.Tags).HasColumnType("text[]"); // Postgres text array
            entity.Property(e => e.Visibility).IsRequired()
                .HasConversion<string>(); // Store enum as string
            entity.Property(e => e.ModelConfigJson).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // Configure Chat entity
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().HasMaxLength(26); // Ulid is 26 chars
            entity.Property(e => e.CharacterId).IsRequired().HasMaxLength(26);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.MemoryStateJson);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            
            // Configure relationships
            entity.HasOne(e => e.Character)
                .WithMany(c => c.Chats)
                .HasForeignKey(e => e.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Message entity
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired().HasMaxLength(26); // Ulid is 26 chars
            entity.Property(e => e.ChatId).IsRequired().HasMaxLength(26);
            entity.Property(e => e.Role).IsRequired()
                .HasConversion<string>(); // Store enum as string
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Tokens);
            entity.Property(e => e.Flagged).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Configure relationships
            entity.HasOne(e => e.Chat)
                .WithMany(c => c.Messages)
                .HasForeignKey(e => e.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}