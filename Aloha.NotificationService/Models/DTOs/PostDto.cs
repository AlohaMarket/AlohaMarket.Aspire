namespace Aloha.NotificationService.Models.DTOs
{
    public class PostDto
    {
        public string Id { get; set; } = default!;
        public string Title { get; set; } = default!;
        public decimal Price { get; set; }
        public string ThumbnailUrl { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string Currency { get; set; } = "VND";
    }
}