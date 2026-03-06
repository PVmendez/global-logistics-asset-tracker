# Global Logistics Asset Tracker (GLAT)

A full-stack logistics asset management platform built with .NET 9.0, React 18.3, SQL Server, and ANSI C.


## Features

- **Asset Management**: Track containers, vehicles, and machinery
- **Real-time Monitoring**: Health scores with AI-powered calculations
- **Maintenance Alerts**: Automatic scheduling and overdue notifications
- **Telemetry Ingestion**: Sensor data collection (temperature, humidity, pressure, vibration)
- **REST API**: 13+ endpoints with OpenAPI/Swagger documentation
- **Clean Architecture**: Layered design for maintainability
- **Native Performance**: C library integration via P/Invoke
- **JWT Authentication**: Secure API access

## Tech Stack

| Component | Technology |
|-----------|-----------|
| **Backend** | .NET 9.0, ASP.NET Core, Entity Framework Core |
| **Frontend** | React 18.3, Vite, TailwindCSS, Zustand |
| **Database** | SQL Server 2022 |
| **Native** | ANSI C (health calculations) |
| **Deployment** | Docker Compose |

## Quick Start

```bash
# Clone and navigate
git clone <repo>
cd GLAT

# Start everything (3 containers)
docker-compose up --build

# Access points
Frontend:  http://localhost:3000     (demo / demo123)
API Docs:  http://localhost:5000/swagger
```

**→ For detailed setup, see [QUICK_START.md](./QUICK_START.md)**


## Development

### Without Docker
```bash
# Backend
cd src/backend/Lexart.GLAT.Api
dotnet run

# Frontend
cd src/frontend/glat-web
npm install && npm run dev

# Native (Linux only)
cd src/native/c
make
```

## Docker Commands

```bash
docker-compose ps           # Status
docker-compose logs -f api  # Logs
docker-compose down -v      # Stop all
```

## Documentation

- **[QUICK_START.md](./QUICK_START.md)** - Installation & usage guide
- **[IMPLEMENTATION_NOTES.md](./IMPLEMENTATION_NOTES.md)** - Technical details
- **[AI_STRATEGY.md](./AI_STRATEGY.md)** - Development methodology

## Demo Credentials

```
Username: demo
Password: demo123
```

## Default Ports

| Service | Port |
|---------|------|
| Frontend | 3000 |
| API | 5000 |
| SQL Server | 1433 |

## Built with AI-Assisted Development

See [AI_STRATEGY.md](./AI_STRATEGY.md) for methodology and tools used.
