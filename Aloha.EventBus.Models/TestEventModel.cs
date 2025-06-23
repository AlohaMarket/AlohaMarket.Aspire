using Aloha.EventBus.Events;

namespace Aloha.EventBus.Models
{
    public class TestSendEventModel : IntegrationEvent
    {
        public string Message { get; set; } = default!;
        public string FromService { get; set; } = default!;
        public string ToService { get; set; } = default!;
    }

    public class TestReceiveEventModel : IntegrationEvent
    {
        public string Message { get; set; } = default!;
        public string FromService { get; set; } = default!;
        public string ToService { get; set; } = default!;
    }
}
