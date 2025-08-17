using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using CharacterChatApi.Data;
using CharacterChatApi.Models;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Configure Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT Options
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing");
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

// Configure JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
        };
    });

builder.Services.AddAuthorization();

// Configure Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Character Chat API", 
        Version = "v1",
        Description = "A .NET 8 Web API for character chat functionality"
    });

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Character Chat API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Health endpoint
app.MapGet("/health", () => new HealthResponse("ok"))
    .WithName("GetHealth")
    .WithOpenApi()
    .WithTags("Health");

// Users endpoints
app.MapGet("/users", [Authorize] async (AppDbContext context) =>
{
    var users = await context.Users
        .Select(u => new UserResponse(u.Id, u.Email, u.Name, u.Role, u.CreatedAt))
        .ToListAsync();
    return Results.Ok(users);
})
.WithName("GetUsers")
.WithOpenApi()
.WithTags("Users");

app.MapPost("/users", async (CreateUserRequest request, AppDbContext context) =>
{
    var user = new User
    {
        Email = request.Email,
        Name = request.Name,
        Role = "User", // Default role
        CreatedAt = DateTime.UtcNow
    };

    context.Users.Add(user);
    await context.SaveChangesAsync();

    var response = new UserResponse(user.Id, user.Email, user.Name, user.Role, user.CreatedAt);
    return Results.Created($"/users/{user.Id}", response);
})
.WithName("CreateUser")
.WithOpenApi()
.WithTags("Users");

// Character endpoints
app.MapGet("/api/characters", async (AppDbContext context, string? q = null, int skip = 0, int take = 10) =>
{
    var query = context.Characters
        .Where(c => c.Visibility == Visibility.PUBLIC);

    if (!string.IsNullOrEmpty(q))
    {
        query = query.Where(c => EF.Functions.ILike(c.Title, $"%{q}%") || EF.Functions.ILike(c.Summary, $"%{q}%"));
    }

    var characters = await query
        .Skip(skip)
        .Take(take)
        .Select(c => new CharacterResponse(
            c.Id, c.Slug, c.Title, c.Summary, c.AvatarUrl, c.SystemPrompt, 
            c.AuthorNote, c.StyleGuidance, c.ExampleDialog, c.Tags, 
            c.Visibility, c.ModelConfigJson, c.CreatedAt, c.UpdatedAt))
        .ToListAsync();

    return Results.Ok(characters);
})
.WithName("GetCharacters")
.WithOpenApi()
.WithTags("Characters");

app.MapGet("/api/characters/{slug}", async (string slug, AppDbContext context) =>
{
    var character = await context.Characters
        .Where(c => c.Slug == slug)
        .Select(c => new CharacterResponse(
            c.Id, c.Slug, c.Title, c.Summary, c.AvatarUrl, c.SystemPrompt, 
            c.AuthorNote, c.StyleGuidance, c.ExampleDialog, c.Tags, 
            c.Visibility, c.ModelConfigJson, c.CreatedAt, c.UpdatedAt))
        .FirstOrDefaultAsync();

    return character == null ? Results.NotFound() : Results.Ok(character);
})
.WithName("GetCharacterBySlug")
.WithOpenApi()
.WithTags("Characters");

app.MapPost("/api/characters", [Authorize] async (CreateCharacterRequest request, AppDbContext context) =>
{
    var now = DateTime.UtcNow;
    var character = new Character
    {
        Id = Ulid.NewUlid().ToString(),
        Slug = request.Slug,
        Title = request.Title,
        Summary = request.Summary,
        AvatarUrl = request.AvatarUrl,
        SystemPrompt = request.SystemPrompt,
        AuthorNote = request.AuthorNote,
        StyleGuidance = request.StyleGuidance,
        ExampleDialog = request.ExampleDialog,
        Tags = request.Tags,
        Visibility = request.Visibility ?? Visibility.PUBLIC,
        ModelConfigJson = request.ModelConfigJson ?? """{"provider":"openai","model":"gpt-4o-mini","temperature":0.8}""",
        CreatedAt = now,
        UpdatedAt = now
    };

    context.Characters.Add(character);
    await context.SaveChangesAsync();

    var response = new CharacterResponse(
        character.Id, character.Slug, character.Title, character.Summary, character.AvatarUrl, 
        character.SystemPrompt, character.AuthorNote, character.StyleGuidance, character.ExampleDialog, 
        character.Tags, character.Visibility, character.ModelConfigJson, character.CreatedAt, character.UpdatedAt);
    
    return Results.Created($"/api/characters/{character.Slug}", response);
})
.WithName("CreateCharacter")
.WithOpenApi()
.WithTags("Characters");

// Chat endpoints
app.MapPost("/api/chats", [Authorize] async (CreateChatRequest request, AppDbContext context) =>
{
    // For now, allow explicit userId as specified in requirements
    var userId = request.UserId ?? 1; // Default to user 1 if not specified
    
    var now = DateTime.UtcNow;
    var chat = new Chat
    {
        Id = Ulid.NewUlid().ToString(),
        CharacterId = request.CharacterId,
        UserId = userId,
        CreatedAt = now,
        UpdatedAt = now
    };

    context.Chats.Add(chat);
    await context.SaveChangesAsync();

    var response = new ChatResponse(
        chat.Id, chat.CharacterId, chat.UserId, chat.Title, chat.MemoryStateJson,
        chat.CreatedAt, chat.UpdatedAt, []);
    
    return Results.Created($"/api/chats/{chat.Id}", response);
})
.WithName("CreateChat")
.WithOpenApi()
.WithTags("Chats");

app.MapGet("/api/chats/{id}", async (string id, AppDbContext context) =>
{
    var chat = await context.Chats
        .Include(c => c.Messages)
        .Where(c => c.Id == id)
        .Select(c => new ChatResponse(
            c.Id, c.CharacterId, c.UserId, c.Title, c.MemoryStateJson,
            c.CreatedAt, c.UpdatedAt,
            c.Messages.OrderBy(m => m.CreatedAt)
                .Select(m => new MessageResponse(
                    m.Id, m.ChatId, m.Role, m.Content, m.Tokens, m.Flagged, m.CreatedAt))
                .ToArray()))
        .FirstOrDefaultAsync();

    return chat == null ? Results.NotFound() : Results.Ok(chat);
})
.WithName("GetChatById")
.WithOpenApi()
.WithTags("Chats");

app.Run();
