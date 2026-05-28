# ChatShaker

ChatShaker is E2EE chat application.

## Project Structure

- **ChatShaker.Api**: The ASP.NET Core Web API project.
- **ChatShaker.Application**: Core business logic and MediatR handlers.
- **ChatShaker.Domain**: Entity definitions and core domain interfaces.
- **ChatShaker.Infrastructure**: Implementation of external services, data access, and persistence.
- **ChatShaker.ChatMauiApp**: The .NET MAUI client application for Android, iOS, Windows, and macOS.
- **ChatShaker.Migrator**: A standalone tool for managing database migrations using FluentMigrator.
- **ChatShaker.UnitTests / IntegrationTests**: Comprehensive testing suites.

## Tech Stack

- **Backend**: .NET 9, ASP.NET Core, SignalR, MediatR, FluentValidation, Hangfire, Redis.
- **Database**: SQL Server.
- **Migrations**: FluentMigrator.
- **Frontend**: .NET MAUI (C#, XAML, Prism Library).
- **Infrastructure**: Docker & Docker Compose.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) - for backend
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) - for maui + prism
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [.NET MAUI Workload](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation):
  ```bash
  dotnet workload install maui
  ```

## Getting Started

### 1. Environment Configuration

The API uses environment variables managed via a `.env` file. 

1. Navigate to the `ChatShaker.Api` directory.
2. Copy `.env.example` to `.env`:
   ```bash
   cp .env.example .env
   ```
3. Open `.env` and update the settings (Database connection strings, JWT keys, Email settings, etc.) as needed.

### 2. Launching Infrastructure

The easiest way to start the required services (SQL Server and Redis) is using Docker Compose:

```bash
cd ChatShaker.Api
docker-compose up -d
```

This will start:
- **SQL Server**: Port 1433
- **Redis**: Port 6379
- **ChatShaker API**: Port 8080 (optional, if you want to run the API inside Docker)

### 3. Database Migrations

Before running the application for the first time, you need to apply database migrations.

**Using helper scripts:**
- **Linux/macOS**: `bash tools/migrator-linux.sh`
- **Windows**: `tools\migrator.bat`

**Manual execution:**
```bash
dotnet run --project ChatShaker.Migrator -- "Server=localhost,1433;Database=ChatShaker;User Id=sa;Password=YourPassword;TrustServerCertificate=True" migrate
```

### 4. Running the API

You can run the API from your IDE (Visual Studio, JetBrains Rider, VS Code) or via CLI:

```bash
dotnet run --project ChatShaker.Api
```

- **Swagger UI**: `http://localhost:5000/swagger` (Check `launchSettings.json` for specific ports)
- **Hangfire Dashboard**: `http://localhost:5000/hangfire` (Credentials defined in `.env`)

### 5. Running the MAUI App

The MAUI app is configured to talk to the backend. By default, it uses `http://10.0.2.2:8080` for Android emulators to reach the host machine.

**Via CLI:**
```bash
# For Android
dotnet build ChatShaker.ChatMauiApp -t:Run -f net9.0-android

```

## Local Development Tips

- **API URL**: If you change the API port, remember to update the `BaseAddress` in `ChatShaker.ChatMauiApp/MauiProgram.cs`.
- **Database**: The API project includes a `DataSeeder` that automatically populates the database with initial data upon startup in development environments.
- **Storage**: Uploaded files are stored in the path specified by `FILES_STORAGE` in `.env`.
