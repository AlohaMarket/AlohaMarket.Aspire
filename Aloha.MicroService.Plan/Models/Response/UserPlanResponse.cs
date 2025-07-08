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
        public int RemainPosts { get; set; }
        public int RemainPushes { get; set; }
        public bool IsActive { get; set; }
    }
}
