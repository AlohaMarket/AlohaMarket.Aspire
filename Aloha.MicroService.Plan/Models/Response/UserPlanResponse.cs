namespace Aloha.MicroService.Plan.Models.Response
{
    public class UserPlanResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int PlanId { get; set; }
        public string PlanName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UsedPosts { get; set; }
        public int RemainingPosts { get; set; }
        public int UsedPushes { get; set; }
        public int RemainingPushes { get; set; }
        public bool IsActive { get; set; }
    }
}
