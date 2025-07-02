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

### Render Deployment (Recommended)

This backend is containerized and ready for Render deployment:

1. **Push to GitHub repository**
2. **Connect to Render**:
   - Go to [Render Dashboard](https://dashboard.render.com)
   - Click "New" → "Web Service"
   - Connect your GitHub repository
3. **Configure deployment**:
   - **Build Command**: Leave empty (Docker handles build)
   - **Start Command**: Leave empty (Docker handles startup)
   - **Dockerfile**: `Dockerfile`
4. **Set environment variables**:
   ```
   MONGODB_CONNECTION_STRING=your_mongodb_connection_string
   DATABASE_NAME=carbox
   MQTT_SERVER=your_mqtt_server (optional)
   MQTT_PORT=1883 (optional)
   ```
5. **Deploy**: Render will automatically build and deploy your Docker container

The API will be available at your Render URL (e.g., `https://your-app.onrender.com`)

### Other Deployment Options
- Azure App Service
- AWS Elastic Beanstalk  
- Google Cloud Run
- Any Docker-compatible hosting platform

## Development

The API uses dependency injection and follows clean architecture principles with separate layers for controllers, services, and repositories.

For local development, the API runs on ports 5000 (HTTP) and 5001 (HTTPS) with CORS enabled for all origins.