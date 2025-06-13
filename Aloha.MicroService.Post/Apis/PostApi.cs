using Aloha.MicroService.Post.Infrastructure.Entity;

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

            return group;
        }

        private static async Task<Results<Ok<Infrastructure.Entity.Post>, BadRequest>> CreatePost([AsParameters] ApiServices services, Infrastructure.Entity.Post post)
        {
            if (post == null)
            {
                return TypedResults.BadRequest();
            }

            if (post.Id == Guid.Empty)
                post.Id = Guid.NewGuid();

            post.Status = PostStatus.Draft;
            post.CreatedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;

            await services.DbContext.Posts.AddAsync(post);
            await services.DbContext.SaveChangesAsync();

            await services.EventPublisher.PublishAsync(new PostCreatedIntegrationEvent
            {
                PostId = post.Id,
                UserId = post.UserId,
                UserPlanId = post.UserPlanId,
                Title = post.Title,
                Description = post.Description,
                Price = post.Price,
                CategoryId = post.CategoryId,
                CategoryPath = post.CategoryPath,
                LocationPath = post.LocationPath,
                IsActive = post.IsActive,
                Status = post.Status,
                Priority = post.Priority,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                PushedAt = post.PushedAt,
                Attributes = post.Attributes
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
            post.LocationPath = updatedPost.LocationPath;
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
                LocationPath = post.LocationPath,
                IsActive = post.IsActive,
                Status = post.Status,
                Priority = post.Priority,
                UpdatedAt = post.UpdatedAt,
                PushedAt = post.PushedAt,
                Attributes = post.Attributes
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

            await services.EventPublisher.PublishAsync(new PostStatusChangedIntegrationEvent
            {
                PostId = post.Id,
                PreviousStatus = previousStatus,
                CurrentStatus = newStatus,
                UpdatedAt = post.UpdatedAt
            });

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

            await services.EventPublisher.PublishAsync(new PostActivationChangedIntegrationEvent
            {
                PostId = post.Id,
                IsActive = isActive,
                UpdatedAt = post.UpdatedAt
            });

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

            await services.EventPublisher.PublishAsync(new PostPushedIntegrationEvent
            {
                PostId = post.Id,
                PushedAt = post.PushedAt.Value
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

            await services.EventPublisher.PublishAsync(new PostDeletedIntegrationEvent
            {
                PostId = post.Id,
                UserId = post.UserId
            });

            return TypedResults.Ok();
        }
    }
}
