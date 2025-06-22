using Aloha.EventBus.Models;
using Aloha.MicroService.Post.Infrastructure.Entity;
using Aloha.MicroService.Post.Infrastructure.Request;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Aloha.MicroService.Post.Apis
{
    public static class PostApi
    {
        public static IEndpointRouteBuilder MapPostApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1")
                  .MapPostEndpoints()
                  .WithTags("Post Api");

            return builder;
        }

        public class TestSendEventRequest
        {
            public string Message { get; set; } = default!;
            public string ToService { get; set; } = "UserService";
        }

        public static RouteGroupBuilder MapPostEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("posts", async ([AsParameters] ApiServices services) =>
            {
                return await services.DbContext.Posts.ToListAsync();
            });

            group.MapGet("posts/{id:guid}", async ([AsParameters] ApiServices services, Guid id) =>
            {
                return await services.DbContext.Posts.Where(p => p.Id == id).FirstOrDefaultAsync();
            });

            group.MapPost("posts", CreatePost);
            group.MapPut("posts/{id:guid}", UpdatePost);
            group.MapPut("posts/{id:guid}/status", UpdatePostStatus);
            group.MapPut("posts/{id:guid}/activate", ActivatePost);
            group.MapPut("posts/{id:guid}/push", PushPost);
            group.MapDelete("posts/{id:guid}", DeletePost);
            group.MapPost("events/test-send", async (
                [AsParameters] ApiServices services,
                TestSendEventRequest request) =>
            {
                var @event = new TestSendEventModel
                {
                    Message = request.Message,
                    FromService = "PostService",
                    ToService = request.ToService
                };

                var result = await services.EventPublisher.PublishAsync(@event);

                services.Logger.LogInformation("TestSendEvent sent to Kafka: {@event}, result: {result}", @event, result);

                return TypedResults.Ok(new
                {
                    Success = result,
                    SentEvent = @event
                });
            });
            group.MapPost("posts/create", CreatePostFromRequest);

            return group;
        }

        private static async Task<Results<Ok<Infrastructure.Entity.Post>, BadRequest<string>>> CreatePostFromRequest(
            [AsParameters] ApiServices services,
            PostCreateRequest request)
        {
            if (request == null)
            {
                return TypedResults.BadRequest("Request cannot be null");
            }

            // Create new post entity from request
            var post = new Infrastructure.Entity.Post
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                UserPlanId = request.UserPlanId,
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Currency = request.Currency ?? "VND",
                CategoryId = request.CategoryId,
                CategoryPath = request.CategoryPath,
                ProvinceCode = request.ProvinceCode,
                DistrictCode = request.DistrictCode,
                WardCode = request.WardCode,
                Attributes = request.Attributes,

                // Set initial validation flags to false
                IsLocationValid = false,
                IsCategoryValid = false,
                IsUserPlanValid = false,
                Status = PostStatus.PendingValidation,

                // Set timestamps
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                // Save post to database
                await services.DbContext.Posts.AddAsync(post);
                await services.DbContext.SaveChangesAsync();

                services.Logger.LogInformation("Created post with ID {PostId}", post.Id);

                // // Publish event for validation
                // await services.EventPublisher.PublishAsync(new PostCreatedIntegrationEvent
                // {
                //     PostId = post.Id,
                //     UserId = post.UserId,
                //     UserPlanId = post.UserPlanId,
                //     CategoryId = post.CategoryId,
                //     CategoryPath = post.CategoryPath,
                //     ProvinceCode = post.ProvinceCode,
                //     DistrictCode = post.DistrictCode,
                //     WardCode = post.WardCode
                // });

                // services.Logger.LogInformation(
                //     "Published validation events for post ID {PostId}, location: {Province}/{District}/{Ward}, category path: {CategoryPath}",
                //     post.Id, post.ProvinceCode, post.DistrictCode, post.WardCode, string.Join("/", post.CategoryPath));

                return TypedResults.Ok(post);
            }
            catch (Exception ex)
            {
                services.Logger.LogError(ex, "Error creating post: {ErrorMessage}", ex.Message);
                return TypedResults.BadRequest($"Error creating post: {ex.Message}");
            }
        }

        private static async Task<Results<Ok<Infrastructure.Entity.Post>, BadRequest>> CreatePost([AsParameters] ApiServices services, Infrastructure.Entity.Post post)
        {
            if (post == null)
            {
                return TypedResults.BadRequest();
            }

            if (post.Id == Guid.Empty)
                post.Id = Guid.NewGuid();

            post.Status = PostStatus.PendingValidation;
            post.CreatedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;

            await services.DbContext.Posts.AddAsync(post);
            await services.DbContext.SaveChangesAsync();

            await services.EventPublisher.PublishAsync(new PostCreatedIntegrationEvent
            {
                PostId = post.Id,
                UserId = post.UserId,
                UserPlanId = post.UserPlanId,
                CategoryId = post.CategoryId,
                CategoryPath = post.CategoryPath,

            });

            return TypedResults.Ok(post);
        }

        private static async Task<Results<Ok<Infrastructure.Entity.Post>, NotFound, BadRequest>> UpdatePost([AsParameters] ApiServices services, Guid id, Infrastructure.Entity.Post updatedPost)
        {
            if (updatedPost == null)
            {
                return TypedResults.BadRequest();
            }

            var post = await services.DbContext.Posts.FindAsync(id);
            if (post == null)
            {
                return TypedResults.NotFound();
            }

            // Update post properties
            post.Title = updatedPost.Title;
            post.Description = updatedPost.Description;
            post.Price = updatedPost.Price;
            post.CategoryId = updatedPost.CategoryId;
            post.CategoryPath = updatedPost.CategoryPath;
            post.ProvinceCode = updatedPost.ProvinceCode;
            post.ProvinceText = updatedPost.ProvinceText;
            post.DistrictCode = updatedPost.DistrictCode;
            post.DistrictText = updatedPost.DistrictText;
            post.WardCode = updatedPost.WardCode;
            post.WardText = updatedPost.WardText;
            post.Attributes = updatedPost.Attributes;
            post.UpdatedAt = DateTime.UtcNow;

            await services.DbContext.SaveChangesAsync();

            await services.EventPublisher.PublishAsync(new PostUpdatedIntegrationEvent
            {
                PostId = post.Id,
                UserId = post.UserId,
                UserPlanId = post.UserPlanId,
                Title = post.Title,
                Description = post.Description,
                Price = post.Price,
                CategoryId = post.CategoryId,
                CategoryPath = post.CategoryPath,
                ProvinceCode = post.ProvinceCode,
                DistrictCode = post.DistrictCode,
                WardCode = post.WardCode
            });

            return TypedResults.Ok(post);
        }

        private static async Task<Results<Ok<Infrastructure.Entity.Post>, NotFound, BadRequest>> UpdatePostStatus([AsParameters] ApiServices services, Guid id, PostStatus newStatus)
        {
            var post = await services.DbContext.Posts.FindAsync(id);
            if (post == null)
            {
                return TypedResults.NotFound();
            }

            var previousStatus = post.Status;
            post.Status = newStatus;
            post.UpdatedAt = DateTime.UtcNow;

            await services.DbContext.SaveChangesAsync();
            return TypedResults.Ok(post);
        }

        private static async Task<Results<Ok<Infrastructure.Entity.Post>, NotFound>> ActivatePost([AsParameters] ApiServices services, Guid id, bool isActive)
        {
            var post = await services.DbContext.Posts.FindAsync(id);
            if (post == null)
            {
                return TypedResults.NotFound();
            }

            post.IsActive = isActive;
            post.UpdatedAt = DateTime.UtcNow;

            await services.DbContext.SaveChangesAsync();

            return TypedResults.Ok(post);
        }

        private static async Task<Results<Ok<Infrastructure.Entity.Post>, NotFound>> PushPost([AsParameters] ApiServices services, Guid id)
        {
            var post = await services.DbContext.Posts.FindAsync(id);
            if (post == null)
            {
                return TypedResults.NotFound();
            }

            post.PushedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;

            await services.DbContext.SaveChangesAsync();

            await services.EventPublisher.PublishAsync(new PostPushIntegrationEvent
            {
                PostId = post.Id,
                UserId = post.UserId,
                UserPlanId = post.UserPlanId,
            });

            return TypedResults.Ok(post);
        }

        private static async Task<Results<Ok, NotFound>> DeletePost([AsParameters] ApiServices services, Guid id)
        {
            var post = await services.DbContext.Posts.FindAsync(id);
            if (post == null)
            {
                return TypedResults.NotFound();
            }

            services.DbContext.Posts.Remove(post);
            await services.DbContext.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}
