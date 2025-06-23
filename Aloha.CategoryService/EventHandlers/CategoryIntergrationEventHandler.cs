using Aloha.CategoryService.Services;
using Aloha.EventBus.Abstractions;
using Aloha.EventBus.Models;
using MediatR;

namespace Aloha.CategoryService.EventHandlers
{
    public class CategoryIntegrationEventHandler(
        ILogger<CategoryIntegrationEventHandler> logger,
        IEventPublisher eventPublisher, ICategoryService categoryService) :
        IRequestHandler<PostCreatedIntegrationEvent>
    {
        public async Task Handle(PostCreatedIntegrationEvent request, CancellationToken cancellationToken)
        {
            logger.LogInformation("Validating category path for PostId={PostId}", request.PostId);

            bool isValid = await categoryService.IsValidCategoryPath(request.CategoryPath);

            if (isValid)
            {
                await eventPublisher.PublishAsync(new CategoryPathValidModel
                {
                    PostId = request.PostId
                });
            }
            else
            {
                await eventPublisher.PublishAsync(new CategoryPathInvalidModel
                {
                    PostId = request.PostId,
                    ErrorMessage = "Invalid category path"
                });
            }
        }

    }
}