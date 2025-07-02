# CARBOX App - Progressive Web Application

## Overview

CARBOX is a ride-sharing application built as a Progressive Web Application (PWA) that connects users with autonomous vehicle pods for transportation between stations. The system consists of a React-based frontend PWA and a .NET Core Web API backend with MongoDB integration. The application includes features for finding rides, QR code scanning for vehicle access, real-time mapping with Azure Maps, and MQTT communication for IoT device integration.

## System Architecture

### Frontend Architecture
- **Framework**: React 19.0.0 with Create React App
- **PWA Features**: Service worker implementation for offline functionality and caching
- **UI Components**: Material-UI (@mui/material) for modern interface components
- **Styling**: Bootstrap 5.3.3 for responsive design and layout
- **Routing**: React Router DOM for single-page application navigation
- **State Management**: React hooks (useState, useEffect) for component state

### Backend Architecture
- **Framework**: ASP.NET Core 8.0 Web API
- **Database**: MongoDB with Azure Cosmos DB integration
- **API Documentation**: Swagger/OpenAPI integration
- **IoT Communication**: MQTT protocol support via MQTTnet library
- **Configuration**: JSON-based configuration with environment-specific settings

## Key Components

### Frontend Components
1. **SearchBox**: Main interface for ride searching with origin/destination selection
2. **Map Integration**: Azure Maps component for location services and station visualization
3. **QR Scanner**: HTML5 QR code scanning for vehicle access
4. **Loading & Status Pages**: User feedback during ride matching and arrival notifications
5. **Service Worker**: PWA functionality with offline caching and background sync

### Backend Services
1. **REST API Controllers**: Handle ride requests, station data, and scanner operations
2. **MQTT Service**: IoT device communication for vehicle status and control
3. **MongoDB Integration**: Data persistence for stations, rides, and user interactions
4. **CORS Configuration**: Cross-origin support for PWA-API communication

### External Integrations
1. **Azure Maps**: Geolocation services and interactive mapping
2. **MongoDB Atlas**: Cloud database hosting via Azure Cosmos DB
3. **PWA Deployment**: GitHub Pages deployment pipeline

## Data Flow

### Ride Booking Flow
1. User selects origin/destination stations and departure time
2. Frontend sends search request to backend API
3. Backend queries MongoDB for available rides
4. System returns matching CARBOX vehicles
5. User confirms booking and receives QR code for vehicle access

### Vehicle Access Flow
1. User scans QR code at vehicle location
2. QR scanner sends vehicle ID to backend
3. Backend validates access and sends MQTT command to vehicle
4. Vehicle unlocks and notifies user of successful access

### Real-time Updates
1. MQTT service maintains persistent connection with vehicles
2. Vehicle status updates are broadcasted to relevant users
3. Frontend receives real-time notifications about vehicle arrival

## External Dependencies

### Frontend Dependencies
- **@mui/material**: Modern React UI components
- **azure-maps-control**: Microsoft Azure Maps SDK
- **html5-qrcode**: QR code scanning functionality
- **react-bootstrap**: Bootstrap components for React
- **react-spinners**: Loading animations
- **dayjs**: Date/time manipulation

### Backend Dependencies
- **MongoDB.Driver**: MongoDB database connectivity
- **MQTTnet**: MQTT protocol implementation
- **Newtonsoft.Json**: JSON serialization
- **Swashbuckle.AspNetCore**: API documentation
- **TimeZoneConverter**: Cross-platform timezone handling

## Deployment Strategy

### Frontend Deployment
- **Platform**: GitHub Pages
- **Build Process**: Create React App build system
- **PWA Assets**: Automatic generation of manifest.json and service worker
- **Static Hosting**: Optimized bundle deployment with asset optimization

### Backend Deployment
- **Platform**: Docker containerization for Render deployment
- **Database**: MongoDB Atlas via Azure Cosmos DB
- **API Hosting**: ASP.NET Core 8.0 with containerized deployment
- **Environment Configuration**: Environment variables for production settings
- **API Documentation**: Swagger UI available in all environments for endpoint testing

### Development Workflow
- **Local Development**: React development server with hot reloading
- **API Development**: ASP.NET Core with Swagger documentation
- **Database**: MongoDB connection with environment-specific connection strings
- **Cross-Platform**: Support for both HTTP and HTTPS endpoints

## Recent Changes

- **July 02, 2025**: Successfully separated CARBOX backend into standalone API
  - Created independent .NET Core 8.0 Web API project in `/carbox-backend`
  - Updated namespaces to `CarboxBackend` for clean separation
  - Configured CORS for cross-origin deployment flexibility
  - Backend runs on ports 5000 (HTTP) and 5001 (HTTPS)
  - Complete API structure: Controllers, Models, Services, Repositories
  - MongoDB and MQTT services integrated for vehicle communication
  - Ready for independent deployment and GitHub upload

- **July 02, 2025**: Added Docker deployment support for Render
  - Created Dockerfile with multi-stage build for .NET 8.0 application
  - Configured Swagger UI to be available in production for endpoint testing
  - Updated CORS and HTTPS settings for cloud deployment
  - Added .dockerignore for optimized builds
  - Created comprehensive deployment guide (DEPLOY.md)
  - Backend now ready for Docker-based deployment on Render platform

## Changelog

```
Changelog:
- July 02, 2025. Initial setup and development environment configuration
```

## User Preferences

```
Preferred communication style: Simple, everyday language.
```