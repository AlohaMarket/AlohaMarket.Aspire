namespace Aloha.PostService.Models.Responses
{
    public class PostStatisticsResponse
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Validated { get; set; }
        public int Invalid { get; set; }
        public int Rejected { get; set; }
        public int Archived { get; set; }
        public int Violation { get; set; }
    }
}
