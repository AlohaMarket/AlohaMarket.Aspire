namespace Aloha.MicroService.Plan.Models.Response
{
    public class PlanResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int DurationDays { get; set; }
        public int MaxPosts { get; set; }
        public int MaxPushes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateAt { get; set; }
    }
}
