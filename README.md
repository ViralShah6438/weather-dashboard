# Weather Dashboard

A full-stack weather application built with Angular (NgModule-based) and ASP.NET Core Web API. The application enables users to search for current weather by city, set and persist default locations, and view detailed weather metrics with caching optimization.

## Architecture Overview

### Frontend (Angular 19 + TypeScript)
- **Architecture**: NgModule-based with feature modules (WeatherModule, SettingsModule)
- **State Management**: RxJS Subjects/BehaviorSubjects for reactive data flow
- **Build**: Production and development configurations with environment-based settings
- **Testing**: Jasmine/Karma unit test framework

### Backend (ASP.NET Core 9)
- **Architecture**: Layered (API → Application → Domain → Infrastructure)
- **Weather Data**: Two-step geocoding flow (GeoAPI → OpenWeatherMap API)
- **Performance**: In-memory caching with configurable TTL
- **Security**: CORS enabled for Angular frontend, rate limiting (fixed window, concurrency, global)
- **API Documentation**: Swagger/OpenAPI integration

## Key Features

✅ **Two-Step Weather Lookup**: City name → Geographic coordinates (GeoAPI) → Weather data (OpenWeatherMap)
✅ **Responsive Design**: Mobile-first layout for desktop and mobile viewing
✅ **Default Location Persistence**: Server-side file-based storage with concurrent access control
✅ **Rate Limiting**: Multi-strategy protection (global, fixed window, concurrency-based)
✅ **Caching**: Configurable in-memory caching to reduce API calls
✅ **Error Handling**: Comprehensive error responses with problem details
✅ **CORS Support**: Pre-configured for local development and production

## Repository Layout

```
weather-dashboard/
├── apps/
│   ├── web/                          # Angular Frontend (NgModule-based)
│   │   ├── src/
│   │   │   ├── app/
│   │   │   │   ├── app.module.ts     # Root module
│   │   │   │   ├── features/         # Feature modules (weather, settings)
│   │   │   │   ├── core/             # Shared services and models
│   │   │   │   └── shared/           # Reusable components
│   │   │   └── environments/         # Environment configurations
│   │   ├── package.json
│   │   ├── angular.json
│   │   └── proxy.conf.json
│   │
│   └── api/                          # ASP.NET Core Web API
│       ├── src/
│       │   ├── WeatherDashboard.Api/           # Controllers, DI config
│       │   ├── WeatherDashboard.Application/   # Use cases, services
│       │   ├── WeatherDashboard.Domain/        # Models, exceptions
│       │   └── WeatherDashboard.Infrastructure/ # API clients, persistence
│       ├── tests/                    # Unit tests (xUnit)
│       ├── WeatherDashboard.sln
│       ├── appsettings.json
│       └── NuGet.config
│
├── .github/workflows/                # CI/CD pipeline
├── .gitignore
├── .editorconfig
└── README.md
```

## Prerequisites

### For Frontend Development
- **Node.js**: v20.x or newer
- **npm**: v10.x or newer
- **Angular CLI**: Installed via npm

### For Backend Development
- **.NET SDK**: 9.0 or newer ([download](https://dotnet.microsoft.com/download))
- **Visual Studio Code** or **Visual Studio** (optional, for debugging)

## Installation

### Frontend Setup

```bash
cd apps/web

# Install dependencies
npm install

# For development (runs on port 4201)
npm start

# For production build
npm run build:prod

# Run unit tests
npm test
```

**Frontend runs on**: `http://localhost:4201`

### Backend Setup

```bash
cd apps/api

# Restore NuGet packages
dotnet restore --configfile NuGet.config

# Build solution
dotnet build

# Run tests
dotnet test

# Run the API (runs on https://localhost:5001)
dotnet run --project src/WeatherDashboard.Api
```

**API runs on**: `https://localhost:5001`
**Swagger UI**: `https://localhost:5001/swagger`

## Configuration

### Backend Configuration (appsettings.json)

```json
{
  "OpenWeather": {
    "BaseUrl": "https://api.openweathermap.org",
    "ApiKey": "YOUR_OPENWEATHERMAP_API_KEY"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:4201"]
  },
  "IsCacheEnabled": true,
  "RateLimiting": {
    "Global": {
      "PermitLimit": 200,
      "WindowSeconds": 60,
      "QueueLimit": 5
    },
    "FixedWindow": {
      "PermitLimit": 100,
      "WindowSeconds": 60,
      "QueueLimit": 10
    },
    "Concurrency": {
      "PermitLimit": 50,
      "QueueLimit": 25
    }
  }
}
```

### Frontend Configuration (environment.ts / environment.prod.ts)

```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'https://localhost:5001/api'
};
```

### Environment Variables (Optional)

**Backend**:
```bash
# For development
set ASPNETCORE_ENVIRONMENT=Development
set OpenWeather__ApiKey=your_api_key_here
```

## API Endpoints

### Weather Endpoints

**GET** `/api/weather?city={cityName}`
- Retrieves current weather for a specified city
- **Parameters**: `city` (string, required)
- **Response**: WeatherResponse object with temperature, humidity, wind speed, icon, description
- **Status Codes**: 200 (OK), 400 (Bad Request), 404 (City not found), 502 (Provider error)

### Settings Endpoints

**GET** `/api/settings/default-location`
- Retrieves the saved default location
- **Response**: `{ "city": "London" }`
- **Status Code**: 200 (OK)

**PUT** `/api/settings/default-location`
- Updates the default location
- **Body**: `{ "city": "Paris" }`
- **Status Code**: 204 (No Content), 400 (Bad Request)

## Getting an OpenWeatherMap API Key

1. Visit [openweathermap.org](https://openweathermap.org/api)
2. Sign up for a free account
3. Navigate to API Keys section
4. Copy your API key
5. Add it to `appsettings.json` or set the environment variable `OpenWeather__ApiKey`

## Running Both Applications

### Option 1: Separate Terminal Windows

**Terminal 1 - Frontend**:
```bash
cd apps/web
npm start
```

**Terminal 2 - Backend**:
```bash
cd apps/api
dotnet run --project src/WeatherDashboard.Api
```

### Option 2: Using VS Code Tasks

Both projects can be launched via VS Code's Run and Debug configuration.

## Testing

### Frontend Unit Tests
```bash
cd apps/web
npm test                    # Watch mode
npm test -- --watch=false  # Single run
```

Tests cover:
- Search bar component (input validation, emit)
- Weather display component (rendering)
- Default location component (persistence logic)
- Dashboard page (API integration, error handling)

### Backend Unit Tests
```bash
cd apps/api
dotnet test WeatherDashboard.sln
```

Tests cover:
- WeatherService behavior
- DefaultLocationService persistence
- Controller validation and responses

## Project Structure Details

### Frontend (NgModule Architecture)

```
src/app/
├── app.module.ts              # Root module
├── app-routing.module.ts      # Main routing
├── app.component.ts           # Root component
├── core/
│   ├── api/
│   │   ├── weather.service.ts
│   │   └── settings.service.ts
│   └── models/
│       └── weather.models.ts
├── features/
│   ├── weather/
│   │   ├── weather.module.ts
│   │   ├── components/
│   │   │   ├── search-bar/
│   │   │   └── weather-display/
│   │   └── pages/
│   │       └── dashboard-page/
│   └── settings/
│       ├── settings.module.ts
│       └── components/
│           └── default-location/
└── shared/
    └── components/
        └── loading-state/

src/environments/
├── environment.ts              # Development config
├── environment.prod.ts         # Production config
├── environment.local.ts        # Local overrides
└── environment.hosted.sample.ts  # Sample hosted config
```

### Backend (Layered Architecture)

```
src/
├── WeatherDashboard.Api/
│   ├── Controllers/            # API endpoints
│   ├── Contracts/              # Request/response DTOs
│   ├── Program.cs              # DI & middleware setup
│   ├── appsettings.json
│   └── appsettings.Development.json
├── WeatherDashboard.Application/
│   ├── Abstractions/           # Service interfaces
│   └── Services/               # Business logic
├── WeatherDashboard.Domain/
│   ├── Models/                 # Domain entities
│   └── Exceptions/             # Custom exceptions
└── WeatherDashboard.Infrastructure/
    ├── Clients/                # OpenWeather API client (with GeoAPI)
    └── Persistence/            # File-based repository

tests/
├── WeatherDashboard.Api.Tests/          # Controller tests
└── WeatherDashboard.Application.Tests/  # Service tests
```

## Technologies & Dependencies

### Frontend
- **Angular**: 19.x (Modules, dependency injection)
- **RxJS**: 7.8+ (Observable streams, Subjects)
- **TypeScript**: 5.6+
- **Karma/Jasmine**: Unit testing
- **SCSS**: Styling

### Backend
- **.NET**: 9.0
- **ASP.NET Core**: Web API framework
- **xUnit**: Unit testing framework
- **Swashbuckle**: Swagger/OpenAPI documentation

## Contributing

When making changes:
1. Maintain the layered architecture on the backend
2. Keep feature modules isolated on the frontend
3. Write tests alongside code
4. Follow the existing code style (use `.editorconfig`)
5. Update this README if adding new features or endpoints

## References

- [Angular Documentation](https://angular.io/docs)
- [OpenWeatherMap API](https://openweathermap.org/api)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [RxJS Documentation](https://rxjs.dev/)
---

**Last Updated**: March 16, 2026  
**Version**: 1.0 - Complete implementation with production-ready patterns
