# Aloha Market (Aspire)

Aloha Market is a .NET 9 microservices backend for a marketplace platform. The solution is orchestrated with .NET Aspire, uses Kafka for async integration, and exposes HTTP APIs through a YARP API gateway.

## Purpose
Build a modular marketplace backend that supports:
- User profiles, roles, and JWT-based authentication (Keycloak)
- Marketplace listings with media uploads, search, and moderation workflows
- Category and location validation with seed data
- Subscription plans and user plan provisioning
- VNPay and Momo payment flows with callbacks
- Real-time chat and notifications over SignalR
- Event-driven service communication and independent data ownership

## Architecture overview
- .NET Aspire AppHost orchestrates services and infrastructure resources.
- API Gateway (YARP) provides a single entry point and forwards auth headers.
- Kafka-based event bus publishes typed integration events per service topic.
- PostgreSQL stores core CRUD data; MongoDB stores location, payment, and chat data.
- ServiceDefaults adds OpenTelemetry, health checks, service discovery, and resilience.

## Aspire resources
AppHost references Kafka, MongoDB, PostgreSQL, Azure SignalR, and RabbitMQ Aspire packages. Only Kafka is wired in code today; database resources are commented out, so supply external connection strings.

## Services

### Runtime services

| Service | Project | Responsibilities | Data store | Events / integrations |
| --- | --- | --- | --- | --- |
| API Gateway | `Aloha.ApiGateway` | Reverse proxy, CORS, auth header forwarding, request logging | None | Keycloak JWT, YARP routes |
| User Service | `Aloha.UserService` (`Aloha.MicroService.User`) | User profiles, avatars, admin status, seller info | PostgreSQL (EF Core) | Cloudinary, Kafka |
| Post Service | `Aloha.MicroService.Post` | Listings, search/filtering, status changes, report/violation workflows | PostgreSQL (EF Core) | Cloudinary, Kafka |
| Category Service | `Aloha.CategoryService` (`Aloha.MicroService.Category`) | Category CRUD, hierarchy path, seed from `Data/categories.json` | PostgreSQL (EF Core) | Kafka |
| Location Service | `Aloha.LocationService` (`Aloha.MicroService.Location`) | Provinces/districts/wards lookup, seed from `Data/provinces.json` | MongoDB | Kafka |
| Plan Service | `Aloha.MicroService.Plan` | Subscription plans, user plan history, admin reporting | PostgreSQL (EF Core) | Kafka, Keycloak roles |
| Payment Service | `Aloha.MicroService.Payment` | Payment records, VNPay and Momo URLs, callback handling | MongoDB | Kafka (CreateUserPlanCommand) |
| Notification Service | `Aloha.NotificationService` | Conversations, messages, SignalR hub (`/notificationHub`) | MongoDB | Kafka, SignalR |

### Shared libraries
- `Aloha.EventBus`: integration event abstractions and MediatR-based dispatch.
- `Aloha.EventBus.Kafka`: Kafka producer/consumer wiring using Aspire.Confluent.Kafka.
- `Aloha.EventBus.Models`: typed event contracts (posts, plans, payments, chat, validation).
- `Aloha.Security`: Keycloak JWT auth extension and claim helpers.
- `Aloha.Shared`: API response helpers, validation attributes, converters, error handling.
- `Aloha.ServiceDefaults`: OpenTelemetry, health checks, service discovery, Cloudinary, and shared DI.
- `Aloha/Aloha.AppHost`: Aspire AppHost that wires all services and Kafka topics.

## Event-driven integration
- Each service publishes to a Kafka topic named after its Aspire project (underscores are replaced with dashes).
- AppHost sets `EVENT_PUBLISHING_TOPICS` and `EVENT_CONSUMING_TOPICS` per service and creates topics for Post, User, Location, Category, Payment, Plan, and Notification.
- Key event groups include:
  - Post create/update/push events and plan rollback.
  - Category and location validation results.
  - Plan provisioning and validation results.
  - Payment-to-plan provisioning commands.
  - Chat user/post info lookup events.

## Technology stack

### Technology
- .NET 9 / C# / ASP.NET Core (controller APIs + minimal endpoints)
- .NET Aspire (AppHost orchestration, service discovery, resilience)
- Apache Kafka (async integration)
- YARP Reverse Proxy (API gateway)
- PostgreSQL + EF Core (Npgsql)
- MongoDB
- Keycloak JWT auth with role-based access (ALOHA_ADMIN, ALOHA_USER)
- OpenTelemetry (metrics, traces, logs)
- SignalR (notifications and chat)
- Cloudinary + ImageSharp (media storage and optimization)
- Docker (Kafka, optional database containers)

### Package
- NuGet: Aspire.Hosting.AppHost, Aspire.Hosting.Kafka, Aspire.Hosting.MongoDB, Aspire.Hosting.PostgreSQL, Aspire.Hosting.Azure.SignalR, Aspire.Hosting.RabbitMQ
- NuGet: Aspire.Confluent.Kafka
- NuGet: MediatR, AutoMapper
- NuGet: Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.Tools, Npgsql.EntityFrameworkCore.PostgreSQL
- NuGet: MongoDB.Driver, MongoDB.Bson
- NuGet: Microsoft.AspNetCore.Authentication.JwtBearer, Microsoft.AspNetCore.OpenApi, Microsoft.OpenApi.Readers
- NuGet: Yarp.ReverseProxy
- NuGet: Swashbuckle.AspNetCore.Swagger, Swashbuckle.AspNetCore.SwaggerGen, Swashbuckle.AspNetCore.SwaggerUI
- NuGet: OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.Extensions.Hosting, OpenTelemetry.Instrumentation.AspNetCore, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.Runtime
- NuGet: AspNetCore.HealthChecks.NpgSql, Microsoft.Extensions.ServiceDiscovery, Microsoft.Extensions.Http.Resilience, Microsoft.Extensions.Logging.Abstractions, Microsoft.Extensions.Configuration.Abstractions, Microsoft.Extensions.DependencyInjection.Abstractions
- NuGet: CloudinaryDotNet, SixLabors.ImageSharp, dotenv.net
- npm: commitizen, cz-conventional-changelog

## Configuration

### Kafka (event bus)
- `EVENT_PUBLISHING_TOPICS` and `EVENT_CONSUMING_TOPICS` are set automatically by AppHost. Set them manually when running a single service.
- When running outside Aspire, provide a Kafka connection string named `kafka` (bootstrap servers).

### API Gateway (YARP)
- Add `ReverseProxy` routes and clusters in configuration (appsettings or environment variables). This repo does not include a default routes file.

### Keycloak JWT
Used by API Gateway, User, Post, and Plan services.
- `Authentication:Authority`
- `Authentication:Audience`

### Cloudinary (Post and User services)
Loaded from a root `.env` file or environment variables.
- `CLOUDINARY_CLOUDNAME`
- `CLOUDINARY_APIKEY`
- `CLOUDINARY_APISECRET`

### PostgreSQL (User, Post, Category, Plan)
- `ConnectionStrings:SupabaseConnection` (primary)
- `ConnectionStrings:DefaultConnection` (fallback)
- `ConnectionStrings:PostgresConnection` (used by AddSharedServicesLocal)

### MongoDB
- Location service: `MongoSettings:ConnectionString`, `MongoSettings:DatabaseName`
- Payment service: `MongoSettings:ConnectionString`, `MongoSettings:DatabaseName`, `MongoSettings:CollectionName`
- Notification service: `MongoDbSettings:ConnectionString`, `MongoDbSettings:DatabaseName`

### Payment providers
- VNPay: `Vnpay:Version`, `Vnpay:Command`, `Vnpay:TmnCode`, `Vnpay:CurrCode`, `Vnpay:Locale`, `Vnpay:BaseUrl`, `Vnpay:HashSecret`
- VNPay callback: `PaymentCallBack:ReturnUrl`, `TimeZoneId`
- Momo: `MomoAPI:PartnerCode`, `MomoAPI:AccessKey`, `MomoAPI:SecretKey`, `MomoAPI:MomoApiUrl`, `MomoAPI:ReturnUrl`, `MomoAPI:NotifyUrl`, `MomoAPI:RequestType`
- Frontend redirects: `FrontendRedirect:SuccessUrl`, `FrontendRedirect:FailedUrl`

### OpenTelemetry
- `OTEL_EXPORTER_OTLP_ENDPOINT` enables OTLP export.

## Running locally

### Prerequisites
- .NET 9 SDK
- Docker Desktop (Kafka and optional DB containers)
- PostgreSQL and MongoDB instances
- Keycloak (JWT issuer)
- Cloudinary account (for media uploads)

### Start the full solution with Aspire

```powershell
dotnet run --project Aloha/Aloha.AppHost
```

Kafka is provisioned by Aspire. In non-test runs, Kafka is persistent and includes Kafka UI.

### Run a single service

```powershell
dotnet run --project Aloha.UserService
```

Provide the configuration values listed above for Kafka, databases, and authentication.

### Optional: Post service PostgreSQL container

```powershell
docker compose up -d
```

Uses `docker-compose.yml` in the repo root to start Postgres and pgAdmin.

## API documentation
Each service exposes Swagger UI in development. The UI is hosted at the service root (`/`) with OpenAPI at `/openapi/v1.json`.

## Repository layout
- `Aloha.Aspire.sln`: solution entry point.
- `Aloha/Aloha.AppHost`: Aspire AppHost wiring (Kafka, project references, topic creation).
- `Aloha.ApiGateway`: YARP reverse proxy and auth header forwarding.
- `Aloha.UserService`, `Aloha.MicroService.Post`, `Aloha.CategoryService`, `Aloha.LocationService`, `Aloha.MicroService.Plan`, `Aloha.MicroService.Payment`, `Aloha.NotificationService`: microservices.
- `Aloha.EventBus`, `Aloha.EventBus.Kafka`, `Aloha.EventBus.Models`: event bus and contracts.
- `Aloha.Security`, `Aloha.Shared`, `Aloha/Aloha.ServiceDefaults`: shared cross-cutting components.
- `docker-compose.yml`: local Postgres + pgAdmin for Post service.

## Architecture Report

# Architecture Facts Report (AlohaMarket.Aspire)

1) Microservices inventory  
8 services are wired into the Aspire AppHost; each has its own `Program.cs` entry point. Domains below are taken directly from controllers and gateway configuration.

| Service | Domain/purpose | Entry project path | Aspire wiring |
| --- | --- | --- | --- |
| API Gateway | YARP reverse proxy + auth header forwarding | `Aloha.ApiGateway/Program.cs` | AppHost `AddProjectWithPostfix<Projects.Aloha_ApiGateway>()` |
| User Service | User profiles/admin status/avatar | `Aloha.UserService/Program.cs` | `AddProjectWithPostfix<Projects.Aloha_MicroService_User>().SetupKafka(...)` |
| Post Service | Listings/search/status/push/report | `Aloha.MicroService.Post/Program.cs` | `AddProjectWithPostfix<Projects.Aloha_MicroService_Post>().SetupKafka(...)` |
| Category Service | Category CRUD + hierarchy path | `Aloha.CategoryService/Program.cs` | `AddProjectWithPostfix<Projects.Aloha_MicroService_Category>().SetupKafka(...)` |
| Location Service | Province/ward lookups | `Aloha.LocationService/Program.cs` | `AddProjectWithPostfix<Projects.Aloha_MicroService_Location>().SetupKafka(...)` |
| Plan Service | Plans + user plans | `Aloha.MicroService.Plan/Program.cs` | `AddProjectWithPostfix<Projects.Aloha_MicroService_Plan>().SetupKafka(...)` |
| Payment Service | Payment records + VNPay/Momo endpoints | `Aloha.MicroService.Payment/Program.cs` | `AddProjectWithPostfix<Projects.Aloha_MicroService_Payment>().SetupKafka(...)` |
| Notification Service | Chat + SignalR notifications | `Aloha.NotificationService/Program.cs` | `AddProjectWithPostfix<Projects.Aloha_NotificationService>().SetupKafka(...)` |

2) Kafka usage  
Kafka is implemented via `Aspire.Confluent.Kafka` with a custom MessageEnvelop serialized using `System.Text.Json`. AppHost creates one topic per service (name derived from the project type with underscores replaced by dashes) and sets `EVENT_PUBLISHING_TOPICS`/`EVENT_CONSUMING_TOPICS`. Consumers use explicit group IDs and event type filters.

Topic wiring (derived names):
| Publishing service topic | Consuming topics configured in AppHost |
| --- | --- |
| Aloha-MicroService-User | Aloha-MicroService-Post, Aloha-MicroService-Location, Aloha-NotificationService |
| Aloha-MicroService-Post | Aloha-MicroService-User, Aloha-MicroService-Location, Aloha-MicroService-Plan, Aloha-MicroService-Category, Aloha-NotificationService |
| Aloha-MicroService-Plan | Aloha-MicroService-User, Aloha-MicroService-Post, Aloha-MicroService-Payment |
| Aloha-MicroService-Location | Aloha-MicroService-Post |
| Aloha-MicroService-Category | Aloha-MicroService-Post |
| Aloha-MicroService-Payment | Aloha-MicroService-Plan |
| Aloha-NotificationService | Aloha-MicroService-User, Aloha-MicroService-Post |

Event handling (consumer groups + event types):
| Service | Group ID | AcceptEvent types | PublishAsync event types |
| --- | --- | --- | --- |
| User | aloha-user-service | UserChatRequestEventModel | TestReceiveEventModel, UserProfileResponseEventModel |
| Post | aloha-post-service | LocationValid/Invalid, CategoryPathValid/Invalid, UserPlanValid/Invalid, PostChatRequestEventModel | PostCreatedIntegrationEvent, PostPushIntegrationEvent, RollbackUserPlanEventModel, PostInfoResponseEventModel |
| Category | aloha-category-service | PostCreatedIntegrationEvent | CategoryPathValid/Invalid |
| Location | aloha-location-service | PostCreatedIntegrationEvent | LocationValid/Invalid |
| Plan | aloha-plan-service | PostCreatedIntegrationEvent, RollbackUserPlanEventModel, CreateUserPlanCommand, TestSendEventModel | UserPlanProvisioningResultEvent, UserPlanValid/Invalid |
| Payment | aloha-payment-service | UserPlanProvisioningResultEvent | CreateUserPlanCommand |
| Notification | aloha-notification-service | UserProfileResponseEventModel, PostInfoResponseEventModel | UserChatRequestEventModel, PostChatRequestEventModel |

3) Data stores mapping  
PostgreSQL (EF Core/Npgsql) is used by User/Post/Category/Plan services via shared `AddSharedServices<TContext>()`. MongoDB is used by Location/Payment/Notification services via `MongoClient`. API Gateway has no DB configuration.

| Service | Store | Configuration evidence |
| --- | --- | --- |
| API Gateway | None | No DB config in Program |
| User | PostgreSQL (EF Core) | `AddSharedServices<UserDbContext>()` |
| Post | PostgreSQL (EF Core) | `AddSharedServices<PostDbContext>()` |
| Category | PostgreSQL (EF Core) | `AddSharedServices<CategoryDbContext>()` |
| Plan | PostgreSQL (EF Core) | `AddSharedServices<PlanDbContext>()` |
| Location | MongoDB | `Configure<MongoSettings>` + `MongoClient` |
| Payment | MongoDB | `Configure<MongoSettings>` + `MongoClient` |
| Notification | MongoDB | `Configure<MongoDbSettings>` + `MongoClient` |

4) Real-time & payments  
SignalR is implemented only in NotificationService for chat/conversation notifications. Payment service integrates VNPay and Momo via HTTP endpoints; VNPay uses HMAC signature validation. No SignalR hooks are present in Payment service.

5) Observability  
ServiceDefaults wires OpenTelemetry logging, metrics, and tracing (ASP.NET Core + HttpClient + runtime). OTLP export is enabled when `OTEL_EXPORTER_OTLP_ENDPOINT` is set. No explicit traceparent propagation config was found in gateway or services beyond default instrumentation.

Resume bullets ready
- Orchestrated 8 ASP.NET Core microservices with .NET Aspire AppHost and a YARP-based API gateway.
- Built a Kafka event bus on Aspire.Confluent.Kafka with JSON-serialized envelopes and per-service consumer groups.
- Implemented polyglot persistence: EF Core/PostgreSQL for user/post/category/plan services and MongoDB for location/payment/notification services.
- Delivered real-time chat with SignalR hub events and REST chat endpoints.
- Integrated VNPay and Momo payment callbacks with HMAC signature validation and Kafka-driven plan provisioning.

## License
This project is licensed under the Unlicense License - see `LICENSE.txt`.
