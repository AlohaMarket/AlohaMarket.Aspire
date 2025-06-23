using Aloha.CategoryService.Data;
using Aloha.CategoryService.Repositories;
using Aloha.CategoryService.Services;
using Aloha.EventBus;
using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Kafka;
using Aloha.EventBus.Models;
using Aloha.ServiceDefaults.DependencyInjection;
using Aloha.ServiceDefaults.Hosting;
using Aloha.Shared;
using Aloha.Shared.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Aloha.CategoryService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.AddServiceDefaults();

            // Add services to the container.  
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi  
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Aloha Category Service API",
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

            // Register Kafka producer
            builder.AddKafkaProducer("kafka");

            // Register Kafka event publisher
            var kafkaPublishTopic = builder.Configuration.GetValue<string>(Consts.Env_EventPublishingTopics);
            if (!string.IsNullOrWhiteSpace(kafkaPublishTopic))
            {
                builder.AddKafkaEventPublisher(kafkaPublishTopic);
            }
            else
            {
                builder.Services.AddTransient<IEventPublisher, NullEventPublisher>();
            }

            var kafkaConsumeTopic = builder.Configuration.GetValue<string>(Consts.Env_EventConsumingTopics);
            if (!string.IsNullOrWhiteSpace(kafkaConsumeTopic))
            {
                builder.AddKafkaEventConsumer(options =>
                {
                    options.ServiceName = "CategoryService";
                    options.KafkaGroupId = "aloha-category-service";
                    options.Topics.AddRange(kafkaConsumeTopic.Split(','));
                    options.IntegrationEventFactory = IntegrationEventFactory<PostCreatedIntegrationEvent>.Instance;
                    options.AcceptEvent = e => e.IsEvent<PostCreatedIntegrationEvent>();
                });
            }

            builder.Services.AddSharedServices<CategoryDbContext>(builder.Configuration);
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, Services.CategoryService>();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.AddTransient<DataSeeder>();

            var app = builder.Build();

            app.MapDefaultEndpoints();

            // Configure the HTTP request pipeline.  
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aloha Category Service API V1");
                    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root  
                });
            }

            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
                seeder.Seed();
            }

            app.UseCors("AllowAll");

            app.UseMiddleware<ApiExceptionHandlerMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}