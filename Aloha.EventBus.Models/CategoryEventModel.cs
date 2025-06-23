using Aloha.EventBus.Events;

namespace Aloha.EventBus.Models
{
    public class CategoryPathValidModel : IntegrationEvent
    {
        public Guid PostId { get; set; }
    }

    public class CategoryPathInvalidModel : IntegrationEvent
    {
        public Guid PostId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
