using Aloha.EventBus.Abstractions;
using Aloha.MicroService.Post.Infrastructure.Data;

namespace Aloha.MicroService.Post
{
    public class ApiServices
    {
        public PostDbContext DbContext { get; set; }
        public IEventPublisher EventPublisher { get; set; }
        public ILogger Logger { get; set; }

        public ApiServices(PostDbContext dbContext, IEventPublisher eventPublisher, ILogger<ApiServices> logger)
        {
            DbContext = dbContext;
            EventPublisher = eventPublisher;
            Logger = logger;
        }
    }
}
