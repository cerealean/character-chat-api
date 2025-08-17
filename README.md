# Character Chat API

A .NET 8 Web API built with minimal APIs, Entity Framework Core, PostgreSQL, JWT authentication, and Swagger documentation.

## Features

- ✅ .NET 8 Web API with minimal APIs
- ✅ Entity Framework Core with PostgreSQL
- ✅ JWT Bearer authentication
- ✅ Swagger UI with JWT support
- ✅ Health check endpoint
- ✅ User management endpoints
- ✅ Clean architecture with proper separation of concerns

## Prerequisites

- .NET 8 SDK
- PostgreSQL database

## Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/cerealean/character-chat-api.git
   cd character-chat-api
   ```

2. **Configure application settings**
   
   The `appsettings.json` file contains placeholder values for security. You have several options for configuration:

   **Option A: Use appsettings.Development.json (for development)**
   
   The development settings file already contains working values for local development.

   **Option B: Use User Secrets (recommended for development)**
   ```bash
   cd CharacterChatApi
   dotnet user-secrets init
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=character_chat_api;Username=your_username;Password=your_password"
   dotnet user-secrets set "Jwt:SecretKey" "your-secure-jwt-secret-key-minimum-32-characters"
   ```

   **Option C: Use Environment Variables (recommended for production)**
   ```bash
   export ConnectionStrings__DefaultConnection="Host=localhost;Database=character_chat_api;Username=your_username;Password=your_password"
   export Jwt__SecretKey="your-secure-jwt-secret-key-minimum-32-characters"
   ```

   **⚠️ Security Note:** Never commit real database credentials or JWT secrets to source control.

3. **Run the application**
   ```bash
   cd CharacterChatApi
   dotnet run
   ```

4. **Access Swagger UI**
   
   Open your browser and navigate to: `http://localhost:5245/swagger`

## API Endpoints

### Health
- **GET** `/health` - Returns API health status

### Users
- **GET** `/users` - Get all users (requires JWT authentication)
- **POST** `/users` - Create a new user

## Authentication

The API uses JWT Bearer authentication. To access protected endpoints:

1. Click the "Authorize" button in Swagger UI
2. Enter your JWT token in the format: `Bearer your_token_here`

## Database Schema

### User Entity
- `Id` (int) - Primary key
- `Email` (string) - Unique email address
- `Name` (string) - User's full name
- `Role` (string) - User role (default: "User")
- `CreatedAt` (DateTime) - Creation timestamp

## Development Notes

- The JWT secret key in `appsettings.json` is for development only
- Update the JWT configuration for production use
- Database migrations will need to be created and applied when setting up the database
- The connection string placeholder assumes a local PostgreSQL instance

## Project Structure

```
CharacterChatApi/
├── Data/
│   └── AppDbContext.cs          # Entity Framework DbContext
├── Models/
│   ├── User.cs                  # User entity model
│   ├── JwtOptions.cs           # JWT configuration model
│   └── ApiModels.cs            # API DTOs
├── Program.cs                   # Application entry point and configuration
├── appsettings.json            # Configuration settings
└── CharacterChatApi.csproj     # Project file
```

## License

This project is licensed under the MIT License.