using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Aloha.ApiGateway.Services
{
    public class SwaggerMergeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public SwaggerMergeService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<OpenApiDocument> GetMergedSwaggerAsync()
        {
            var mergedDoc = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Title = "Unified Aloha API",
                    Version = "v1",
                    Description = "Merged OpenAPI from all downstream services"
                },
                Paths = new OpenApiPaths(),
                Components = new OpenApiComponents(),
                Tags = new List<OpenApiTag>(),
                Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = _configuration["GatewayBaseUrl"] ?? "https://localhost:7000" }
                }
            };

            var reader = new OpenApiStringReader();
            var downstreamUrls = _configuration
                .GetSection("DownstreamSwaggerUrls")
                .Get<Dictionary<string, string>>();

            foreach (var (serviceName, url) in downstreamUrls)
            {
                var client = _httpClientFactory.CreateClient();
                string json;

                try
                {
                    json = await client.GetStringAsync(url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to fetch Swagger from {serviceName}: {ex.Message}");
                    continue;
                }

                var doc = reader.Read(json, out var diagnostic);
                if (diagnostic?.Errors.Count > 0)
                {
                    Console.WriteLine($"Warning: Errors reading Swagger from {serviceName}");
                }

                if (!mergedDoc.Tags.Any(t => t.Name == serviceName))
                {
                    mergedDoc.Tags.Add(new OpenApiTag { Name = serviceName });
                }

                foreach (var (pathKey, pathItem) in doc.Paths)
                {
                    // Avoid conflict by prefixing with service name if desired
                    // var mergedPathKey = $"/{serviceName}{pathKey}";

                    var mergedPathKey = pathKey;
                    if (!mergedDoc.Paths.ContainsKey(mergedPathKey))
                    {
                        mergedDoc.Paths[mergedPathKey] = pathItem;

                        // Retag each operation for the service
                        foreach (var op in pathItem.Operations)
                        {
                            op.Value.Tags = new List<OpenApiTag> { new OpenApiTag { Name = serviceName } };
                        }
                    }
                }

                // Optionally merge components (schemas, responses, etc.)
                // You should handle name conflicts if you go this route
            }

            return mergedDoc;
        }
    }
}
