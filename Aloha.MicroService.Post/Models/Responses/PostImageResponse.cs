namespace Aloha.PostService.Models.Responses
{
    public class PostImageResponse
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
