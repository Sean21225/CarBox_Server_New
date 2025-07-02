# CARBOX Backend API

A .NET Core Web API backend for the CARBOX ride-sharing application with autonomous vehicle pods.

## Features

- **RESTful API** for ride booking and vehicle management
- **MongoDB integration** for data persistence
- **MQTT service** for IoT device communication with vehicles  
- **Swagger documentation** for API endpoints
- **CORS support** for frontend integration

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: MongoDB with Azure Cosmos DB support
- **IoT Communication**: MQTT protocol via MQTTnet
- **Documentation**: Swagger/OpenAPI

## Project Structure

```
carbox-backend/
├── Controllers/           # API controllers
├── Models/               # Data models
├── Services/             # Business logic services
├── Repositories/         # Data access layer
├── Date/                # Database and MQTT services
├── Common/              # Global constants
├── Properties/          # Launch settings
└── appsettings.json     # Configuration
```

## API Endpoints

- `GET /api/stations` - Get all available stations
- `POST /api/RideOrders` - Create a new ride order
- `GET /api/cars` - Get available vehicles
- `POST /api/scanner` - Handle QR code scanning
- `POST /api/StartStop` - Start/stop vehicle operations

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- MongoDB connection string
- MQTT broker (optional)

### Installation

1. Clone the repository
```bash
git clone <your-repo-url>
cd carbox-backend
```

2. Restore dependencies
```bash
dotnet restore
```

3. Update connection strings in `appsettings.json`

4. Run the application
```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001/swagger`

## Configuration

Update `appsettings.json` with your MongoDB connection string and other settings:

```json
{
  "ConnectionString": "your-mongodb-connection-string",
  "DatabaseName": "carbox"
}
```

## Deployment

This backend is designed to be deployed independently and can be hosted on:
- Azure App Service
- AWS Elastic Beanstalk  
- Docker containers
- Any .NET Core compatible hosting platform

## Development

The API uses dependency injection and follows clean architecture principles with separate layers for controllers, services, and repositories.

For local development, the API runs on ports 5000 (HTTP) and 5001 (HTTPS) with CORS enabled for all origins.