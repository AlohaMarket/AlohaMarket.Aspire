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
        private static class AppConsts
        {
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
            });            // Cau hinh co so du lieu duoc xu ly trong AddAlohaPostgreSQL

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
            string configSection = null) where TContext : DbContext
        {
            // Xác định phần cấu hình từ kiểu context nếu không được cung cấp
            configSection ??= typeof(TContext).Name.Replace("DbContext", "");
            
            // Xác định chuỗi kết nối theo thứ tự ưu tiên
            string connectionString;
            
            // 1. Kiểm tra chuỗi kết nối hoàn chỉnh từ biến môi trường
            connectionString = Environment.GetEnvironmentVariable($"Aloha_{configSection}_ConnectionString");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // 2. Kiểm tra chuỗi kết nối từ appsettings.json
                connectionString = builder.Configuration.GetConnectionString($"{configSection}Connection") ??
                                  builder.Configuration.GetConnectionString("SupabaseConnection");
                                  
                if (string.IsNullOrEmpty(connectionString))
                {
                    // 3. Lấy cấu hình từ phần Database trong appsettings.json
                    var dbConfig = builder.Configuration.GetSection($"Database:{configSection}");
                    
                    // Lấy chuỗi kết nối template với placeholder [YOUR-PASSWORD]
                    var connectionStringTemplate = dbConfig["ConnectionStringTemplate"] ??
                        "User Id=postgres.atdfjkewwxqzwoyizhch;Password=[YOUR-PASSWORD];Server=aws-0-ap-southeast-1.pooler.supabase.com;Port=5432;Database=postgres";
                    
                    // Lấy mật khẩu theo thứ tự ưu tiên
                    var password = Environment.GetEnvironmentVariable($"{configSection}_DB_PASSWORD") ??
                                  dbConfig["Password"] ?? 
                                  "postgres";
                    
                    // Thay thế [YOUR-PASSWORD] bằng mật khẩu thực tế sử dụng Replace
                    connectionString = connectionStringTemplate.Replace("[YOUR-PASSWORD]", password);
                }
            }
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    $"Không tìm thấy ConnectionString cho {configSection}. " +
                    $"Vui lòng kiểm tra biến môi trường, phần ConnectionStrings hoặc Database trong appseting.json.");
            }
            
            // Ghi log thông tin kết nối (đã được ẩn thông tin nhạy cảm)
            var sanitizedConnectionString = connectionString.Replace(
                new Regex("Password=([^;]*)").Match(connectionString).Value,
                "Password=*****");
            
            var logger = builder.Services.BuildServiceProvider().GetService<ILogger<object>>();
            logger?.LogInformation(
                "Cấu hình cơ sở dữ liệu cho {ContextType} sử dụng phần '{ConfigSection}'", 
                typeof(TContext).Name, configSection);
            
            // Không ghi log chuỗi kết nối đầy đủ để tránh lộ thông tin nhạy cảm
            logger?.LogDebug("ConnectionString: {ConnectionString}", sanitizedConnectionString);
            
            // Cấu hình DbContext với chuỗi kết nối và chính sách retry
            builder.Services.AddDbContext<TContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                }));
            
            // Đăng ký DbContext cho dependency injection
            builder.Services.AddScoped<TContext>();
            
            return builder;
        }
    }
}
