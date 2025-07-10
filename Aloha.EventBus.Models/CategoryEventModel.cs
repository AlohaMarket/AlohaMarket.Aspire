using Aloha.EventBus.Events;

namespace Aloha.EventBus.Models
{
    public class CategoryPathValidEventModel : IntegrationEvent
    {
        public Guid PostId { get; set; }
    }

    public class CategoryPathInvalidEventModel : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
