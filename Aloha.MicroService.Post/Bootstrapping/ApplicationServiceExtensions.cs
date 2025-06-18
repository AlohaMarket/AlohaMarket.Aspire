using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.MicroService.Post.Infrastructure.Data;
using Aloha.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;


namespace Aloha.MicroService.Post.Bootstrapping
{
    public static class ApplicationServiceExtensions
    {
        private static class AppConsts
        {
            // Hang so cu duoc giu lai de dam bao tinh tuong thich nguoc
            public const string DefaultDatabase = "PostDatabase";
            public const string Env_DbUsername = "DB_USERNAME";
            public const string Env_DbPassword = "DB_PASSWORD";
            public const string Env_DatabaseConnection = "Aloha_PostDB_ConnectionString";
            
            // Cac phuong thuc ho tro dinh dang cho cau hinh dong
            public static string GetConnectionStringEnvName(string service) => $"Aloha_{service}_ConnectionString";
            public static string GetUsernameEnvName(string service) => $"{service}_DB_USERNAME";
            public static string GetPasswordEnvName(string service) => $"{service}_DB_PASSWORD";
            public static string GetDatabaseNameEnvName(string service) => $"{service}_DB_NAME";
        }

        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {            builder.AddServiceDefaults()
                   .AddAlohaPostgreSQL<PostDbContext>("Post"); // Dat ten ro rang cho phan cau hinh

            builder.Services.AddAuthorization();
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.MapType<IFormFile>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                });

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Aloha Post Service API",
                    Version = "v1"
                });

                c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter your token:"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        new List<string>()
                    }
                });
            });            // Cau hinh co so du lieu duoc xu ly trong AddAlohaPostgreSQL

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            });

            // Cau hinh event bus Kafka
            builder.AddKafkaProducer("kafka");

            // Cau hinh Kafka event publisher
            var kafkaTopic = builder.Configuration.GetValue<string>(Consts.Env_EventPublishingTopics);
            if (!string.IsNullOrEmpty(kafkaTopic))
            {
                builder.AddKafkaEventPublisher(kafkaTopic);
            }
            else
            {
                builder.Services.AddTransient<IEventPublisher, NullEventPublisher>();
            }

            var eventConsumingTopics = builder.Configuration.GetValue<string>(Consts.Env_EventConsumingTopics);
            if (!string.IsNullOrEmpty(eventConsumingTopics))
            {
                builder.AddKafkaEventConsumer(options =>
                {
                    options.ServiceName = "PostService";
                    options.KafkaGroupId = "aloha-post-service";
                    options.Topics.AddRange(eventConsumingTopics.Split(','));
                    options.IntegrationEventFactory = IntegrationEventFactory<TestSendEventModel>.Instance;
                    options.AcceptEvent = e => e.IsEvent<
                        TestReceiveEventModel>();
                });
            }

            builder.Logging.AddFilter("Confluent.Kafka", LogLevel.Debug);

            return builder;
        }
        public static IHostApplicationBuilder AddAlohaPostgreSQL<TContext>(
            this IHostApplicationBuilder builder,
            string configSection = null) where TContext : DbContext
        {
            // Xac dinh phan cau hinh tu kieu context neu khong duoc cung cap
            // Loai bo hau to "DbContext" tu ten kieu de co ten dich vu sach
            configSection ??= typeof(TContext).Name.Replace("DbContext", "");
            
            // Lay cau hinh co so du lieu tu phan trong appsettings.json
            var dbConfig = builder.Configuration.GetSection($"Database:{configSection}");
            
            // Lay chuoi ket noi voi thu tu uu tien:
            // 1. Bien moi truong voi ten cu the cho dich vu
            // 2. Cau hinh tu phan Database:ServiceName
            // 3. Phan ConnectionStrings trong appsettings.json
            // 4. Gia tri mac dinh (co the gay loi neu tat ca cach tren deu that bai)
            var connectionStringTemplate = 
                Environment.GetEnvironmentVariable($"Aloha_{configSection}_ConnectionString") ??
                dbConfig["ConnectionString"] ??
                builder.Configuration.GetConnectionString($"{configSection}Database") ??
                "Host=localhost;Database={0};Username={1};Password={2}";
            
            // Lay ten nguoi dung voi thu tu uu tien:
            // 1. Bien moi truong voi ten cu the cho dich vu
            // 2. Cau hinh tu phan Database:ServiceName
            // 3. Gia tri mac dinh
            var username = 
                Environment.GetEnvironmentVariable($"{configSection}_DB_USERNAME") ??
                dbConfig["Username"] ?? 
                "postgres";
            
            // Lay mat khau voi thu tu uu tien:
            // 1. Bien moi truong voi ten cu the cho dich vu
            // 2. Cau hinh tu phan Database:ServiceName
            // 3. Gia tri mac dinh
            var password = 
                Environment.GetEnvironmentVariable($"{configSection}_DB_PASSWORD") ??
                dbConfig["Password"] ?? 
                "postgres";
            
            // Lay ten co so du lieu voi thu tu uu tien
            var database =
                Environment.GetEnvironmentVariable($"{configSection}_DB_NAME") ??
                dbConfig["DatabaseName"] ??
                configSection.ToLowerInvariant();
            
            // Dinh dang chuoi ket noi voi thong tin xac thuc
            // Su dung string.Format de chen ten nguoi dung va mat khau
            var connectionString = string.Format(connectionStringTemplate, database, username, password);
            
            // Ghi log cau hinh dang duoc su dung (da duoc lam sach)
            var logger = builder.Services.BuildServiceProvider().GetService<ILogger<object>>();
            logger?.LogInformation(
                "Configuring database for {ContextType} using section '{ConfigSection}', " +
                "database '{Database}', user '{Username}'", 
                typeof(TContext).Name, configSection, database, username);
            
            // Cau hinh PostgreSQL DbContext voi chuoi ket noi
            builder.Services.AddDbContext<TContext>(options =>
                options.UseNpgsql(connectionString));
            
            // Dang ky DbContext cho dependency injection
            builder.Services.AddScoped<TContext>();
            
            return builder;
        }
    }
}
