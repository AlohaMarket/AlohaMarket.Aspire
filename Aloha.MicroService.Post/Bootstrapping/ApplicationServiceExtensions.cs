using System.Text.RegularExpressions;
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
        private static ILoggerFactory? _loggerFactory;
        
        private static class AppConsts
        {
            public const string DefaultDatabase = "PostDatabase";
            public const string Env_DbUsername = "DB_USERNAME";
            public const string Env_DbPassword = "DB_PASSWORD";
            public const string Env_DatabaseConnection = "Aloha_PostDB_ConnectionString";
            
            // Cac phuong thuc ho tro dinh dang cho cau hinh dong
            public static string GetConnectionStringEnvName(string service) => $"Aloha_{service}_ConnectionString";
            public static string GetUsernameEnvName(string service) => $"Aloha_{service}_Username";
            public static string GetPasswordEnvName(string service) => $"Aloha_{service}_Password";
            public static string GetDatabaseNameEnvName(string service) => $"Aloha_{service}_Name";
        }

        public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
        {
            _loggerFactory = builder.Services.BuildServiceProvider().GetService<ILoggerFactory>();
            
            builder.AddServiceDefaults()
                   .AddAlohaPostgreSQL<PostDbContext>();

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
            });            

            builder.Services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            });

            builder.AddKafkaProducer("kafka");

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
            string? configSection = null) where TContext : DbContext
        {
            // khoi tao lai loggerFactory trong truong hop chua duoc khoi tao
            _loggerFactory ??= builder.Services.BuildServiceProvider().GetService<ILoggerFactory>();

            // CreateLogger tạo một logger mới với tên "DatabaseConfiguration" giúp dễ dàng phân loại và lọc log khi cần thiết.
            var dbLogger = _loggerFactory?.CreateLogger("DatabaseConfiguration");
            
            configSection ??= typeof(TContext).Name.Replace("DbContext", "");
            
            string? connectionString = null;
            string? password = null;

            // 1. Truy xuat bien moi truong du vao DbContext duoc truyen vao
            connectionString = Environment.GetEnvironmentVariable($"Aloha_{configSection}_ConnectionString");
            password = Environment.GetEnvironmentVariable($"Aloha_{configSection}_Password");

            // 2. Kiem tra chuoi ket noi tu appsettings.json neu khong tim thay o moi truong
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = builder.Configuration.GetConnectionString($"{configSection}Connection") ??
                                  builder.Configuration.GetConnectionString("SupabaseConnection") ??
                                  builder.Configuration.GetConnectionString("DefaultConnection");
            }

            // 3. Neu van khong tim thay, su dung gia tri mac dinh va log loi
            if (string.IsNullOrEmpty(connectionString))
            {
                // Sử dụng giá trị mặc định cho Postgres
                connectionString = "User Id=postgres;Password=postgres;Server=localhost;Port=5432;Database=postgres";

                dbLogger?.LogError(
                    "Không tìm thấy ConnectionString cho {ConfigSection}. Sử dụng connection string mặc định.",
                    configSection);
            }

            // Thay the [YOUR-PASSWORD] bang mat khau tu bien moi truong
            if (!string.IsNullOrEmpty(password) && connectionString.Contains("[YOUR-PASSWORD]"))
            {
                connectionString = connectionString.Replace("[YOUR-PASSWORD]", password);
            }

            #region logging trang thai connection
            // Ghi log thông tin kết nối (đã được ẩn thông tin nhạy cảm)
            var sanitizedConnectionString = connectionString;
            var passwordMatch = new Regex("Password=([^;]*)").Match(connectionString);
            if (passwordMatch.Success)
            {
                sanitizedConnectionString = connectionString.Replace(passwordMatch.Value, "Password=*****");
            }

            dbLogger?.LogInformation("Configuring database for {ContextType} using section '{ConfigSection}'", 
                typeof(TContext).Name, configSection);
            dbLogger?.LogDebug("ConnectionString: {ConnectionString}", sanitizedConnectionString);
            #endregion

            #region Add DbContext va retry policy
            builder.Services.AddDbContext<TContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                }));
            #endregion
            
            return builder;
        }
    }
}
