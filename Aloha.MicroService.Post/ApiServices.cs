using Aloha.EventBus.Abstractions;
using Aloha.MicroService.Post.Infrastructure.Data;

namespace Aloha.MicroService.Post
{
    public class ApiServices
    {
        public PostDbContext DbContext { get; set; }
        public IEventPublisher EventPublisher { get; set; }
        public ILogger<ApiServices> Logger { get; set; }

        public ApiServices(PostDbContext DbContext, IEventPublisher EventPublisher, ILogger<ApiServices> Logger)
        {
            this.DbContext = DbContext;
            this.EventPublisher = EventPublisher;
            this.Logger = Logger;
        }
    }
}
