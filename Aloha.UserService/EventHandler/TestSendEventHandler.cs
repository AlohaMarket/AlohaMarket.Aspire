using Aloha.EventBus.Models;
using MediatR;

namespace Aloha.MicroService.User.EventHandler
{
    public class TestSendEventHandler : IRequestHandler<TestSendEventModel>
    {
        private readonly ILogger<TestSendEventHandler> _logger;

        public TestSendEventHandler(ILogger<TestSendEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(TestSendEventModel @event)
        {
            _logger.LogInformation("Received TestSendEventModel: Message={Message}, From={From}, To={To}",
                @event.Message, @event.FromService, @event.ToService);

            return Task.CompletedTask;
        }

        public Task Handle(TestSendEventModel request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received TestSendEventModel: Message={Message}, From={From}, To={To}",
                request.Message, request.FromService, request.ToService);

            return Task.CompletedTask;
        }
    }
}
