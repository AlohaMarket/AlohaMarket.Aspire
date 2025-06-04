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
            var downstreamUrls = _configuration.GetSection("DownstreamSwaggerUrls").Get<Dictionary<string, string>>();
            var openApiReader = new OpenApiStringReader();
            var mergedDoc = new OpenApiDocument
            {
                Info = new OpenApiInfo { Title = "Aloha Gateway API", Version = "v1" },
                Paths = new OpenApiPaths(),
                Components = new OpenApiComponents()
            };

            foreach (var kvp in downstreamUrls)
            {
                var name = kvp.Key;
                var url = kvp.Value;

                using var client = _httpClientFactory.CreateClient();
                var json = await client.GetStringAsync(url);
                var doc = openApiReader.Read(json, out var diagnostic);

                // Prefix paths to avoid conflicts
                foreach (var path in doc.Paths)
                {
                    var prefixedPath = $"/api/{name.ToLower()}{path.Key}";
                    mergedDoc.Paths[prefixedPath] = path.Value;
                }

                // Merge components (you might need deduplication logic for larger projects)
                foreach (var schema in doc.Components.Schemas)
                    mergedDoc.Components.Schemas.TryAdd(schema.Key, schema.Value);

                foreach (var response in doc.Components.Responses)
                    mergedDoc.Components.Responses.TryAdd(response.Key, response.Value);
            }

            return mergedDoc;
        }
    }
}
