using Aloha.CategoryService.Models.Entities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aloha.CategoryService.Data
{
    public class DataSeeder
    {
        private readonly CategoryDbContext context;
        private readonly Dictionary<int, int> oldToNewIdMap; // Maps original IDs to new ones

        public DataSeeder(CategoryDbContext context)
        {
            this.context = context;
            this.oldToNewIdMap = new Dictionary<int, int>();
        }

        public void Seed()
        {
            if (context.Categories.Any()) return;

            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "categories.json");

            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"categories.json not found at {jsonPath}");
            }

            var json = File.ReadAllText(jsonPath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var categories = JsonSerializer.Deserialize<List<JsonCategory>>(json, options);
            if (categories != null && categories.Any())
            {
                SeedCategories(categories);
                context.SaveChanges();
            }
            else
            {
                throw new InvalidOperationException("No categories found in the JSON file.");
            }
        }

        private void SeedCategories(List<JsonCategory> categories, int? parentId = null)
        {
            var sortOrder = 1;
            foreach (var jsonCat in categories)
            {
                var category = new Category
                {
                    // Remove Id assignment to let EF Core generate it
                    Name = jsonCat.Name,
                    DisplayName = jsonCat.DisplayName,
                    ParentId = parentId, // Use the new parent ID passed down
                    Level = jsonCat.Level,
                    SortOrder = sortOrder++,
                    Description = "", // Assuming no description in JSON
                    IsActive = true
                };

                context.Categories.Add(category);
                context.SaveChanges(); // Save to get new generated ID

                // Store mapping between old and new IDs
                oldToNewIdMap[jsonCat.CatId] = category.Id;

                if (jsonCat.Children?.Any() == true)
                {
                    // Pass the new ID as parent for children
                    SeedCategories(jsonCat.Children, category.Id);
                }
            }
        }

        private class JsonCategory
        {
            [JsonPropertyName("catid")]
            public int CatId { get; set; }

            [JsonPropertyName("parent_catid")]
            public int ParentCatId { get; set; }

            public string Name { get; set; }

            [JsonPropertyName("display_name")]
            public string DisplayName { get; set; }

            public int Level { get; set; }
            public List<JsonCategory> Children { get; set; }
        }
    }
}
