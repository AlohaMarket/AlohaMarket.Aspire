# ApplicationServiceExtensions Guide 🛠️

> Extension class để cấu hình và khởi tạo các services cho Aloha Post Microservice


## 🔄 Service Registration Flow

#### Đây là flow để implement các method vào class, các bước cần implement đã được đánh số thứ tự trong diagram

```mermaid
flowchart TB
    %% MAIN APP FLOW
    subgraph AddApplicationServices
        A[Start] --> B[1/ Add Service Defaults]
        B --> C[2/ Configure Basic Services]
        C --> D[3/ Configure Swagger/OpenAPI]
        D --> E[4/ Configure Database]
        E --> F[5/ Configure MediatR]
        F --> G[Configure Kafka]
    end
    style C fill:#E0F7FA,stroke:#00ACC1,stroke-width:2px,color:#006064
    style G fill:#FFF3E0,stroke:#FB8C00,stroke-width:2px,color:#E65100

    %% BASIC SERVICES
    subgraph Configure Basic Services
        C1[2a/ Authorization]
        C2[2b/ OpenAPI]
        C3[2c/ Endpoints API Explorer]
    end
    C --> C1
    C --> C2
    C --> C3

    %% KAFKA BLOCK
    subgraph Configure Kafka
        G1[6/ Configure Producer]
        G2[7/ Configure Event Publisher]
        G3[8/ Configure Event Consumer]
        G4[9/ Configure Logging]
    end
    G --> G1
    G --> G2
    G --> G3
    G --> G4

    %% FLOATING ENV VARS (no direct flow dependency)
    subgraph EnvironmentVariables["🔐 Environment Variables"]
        EV1[DB_USERNAME]
        EV2[DB_PASSWORD]
        EV3[Aloha_PostDB_ConnectionString]
        EV4[EventBus:PublishingTopics]
        EV5[EventBus:ConsumingTopics]
    end
    style EnvironmentVariables fill:#F1F8E9,stroke:#689F38,stroke-width:2px,color:#33691E

    %% Link without enforcing layout
    E -. reads .-> EV1
    E -. reads .-> EV2
    E -. reads .-> EV3
    G2 -. uses .-> EV4
    G3 -. uses .-> EV5

```

## 📊 Dependencies Flow

```mermaid
graph LR
    A[Application] --> B[ServiceDefaults]
    A --> C[Authentication]
    A --> D[Database]
    A --> E[EventBus]
    
    E --> F[Kafka Producer]
    E --> G[Event Publisher]
    E --> H[Event Consumer]
```


## 🔧 Core Configurations

### 1. Swagger/OpenAPI
```csharp
builder.Services.AddSwaggerGen(c => {
    // JWT Authentication
    // File Upload Support
    // API Documentation
});
```

### 2. Database Configuration
```csharp
// Environment Variables
- DB_USERNAME
- DB_PASSWORD
- Aloha_PostDB_ConnectionString
```

### 3. Event Bus Setup
```csharp
// Kafka Configuration
- EventBus:PublishingTopics
- EventBus:ConsumingTopics
- Kafka:BootstrapServers
```

## 📝 Hướng Dẫn Sử Dụng

### 1. Basic Setup
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.AddApplicationServices();
```

### 2. Environment Variables Setup
```powershell
setx DB_USERNAME "your_username"
setx DB_PASSWORD "your_password"
setx Aloha_PostDB_ConnectionString "your_connection_string"
```

### 3. Kafka Topic Configuration
```json
{
  "EventBus": {
    "PublishingTopics": "topic1,topic2",
    "ConsumingTopics": "topic3,topic4"
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

## ⚠️ Lưu Ý Quan Trọng

1. **Security**
   - JWT Authentication được cấu hình sẵn
   - Database credentials nên được quản lý qua environment variables
   - Swagger được bảo vệ bằng JWT

2. **Event Bus**
   - Kafka producer được cấu hình mặc định
   - NullEventPublisher làm fallback khi không có topic
   - Consumer group ID: "aloha-post-service"

3. **Database**
   - PostgreSQL là database mặc định
   - Connection string có thể override qua environment variables

## 🔍 Troubleshooting

### Common Issues
1. **Database Connection**
   ```
   Check:
   - Environment variables
   - PostgreSQL service running
   - Network connectivity
   ```

2. **Kafka Connection**
   ```
   Check:
   - BootstrapServers configuration
   - Topic existence
   - Consumer group registration
   ```

3. **Authentication**
   ```
   Check:
   - JWT configuration
   - Token validity
   - Authorization headers
   ```

## 🚀 Best Practices

1. **Configuration**
   - Sử dụng environment variables cho sensitive data
   - Tách biệt configuration sections
   - Implement fallback values

2. **Event Bus**
   - Định nghĩa rõ publishing và consuming topics
   - Implement error handling
   - Log Kafka operations (Debug level)

3. **Security**
   - Sử dụng JWT authentication
   - Secure database credentials
   - Implement HTTPS

---
Built with 🌺 by Aloha Market Team