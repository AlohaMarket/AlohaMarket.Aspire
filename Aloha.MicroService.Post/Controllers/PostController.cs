using Aloha.PostService.Models.Entity;
using Aloha.PostService.Models.Enums;
using Aloha.PostService.Models.Requests;
using Aloha.PostService.Services;
using Aloha.Shared.Meta;
using Aloha.Shared.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aloha.PostService.Controllers
{
    [Route("api/post")]
    [ApiController]
    public class PostController(IPostService postService) : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> GetAllPosts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? locationId = null,
            [FromQuery] LocationLevel? locationLevel = null, // Use enum here
            [FromQuery] int? categoryId = null,
            [FromQuery] string? searchTerm = null)
        {
            var posts = await postService.GetPostsAsync(searchTerm, locationId, locationLevel, categoryId, page, pageSize);
            return Ok(ApiResponseBuilder.BuildResponse("Posts retrieved successfully!", posts));
        }

        [HttpGet("{postId:guid}")]
        public async Task<IActionResult> GetPostById(Guid postId)
        {
            var post = await postService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound(ApiResponseBuilder.BuildResponse<object>("Post not found", null));

            return Ok(ApiResponseBuilder.BuildResponse("Post retrieved successfully!", post));
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetPostsByUserId(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var posts = await postService.GetPostsByUserIdAsync(userId, page, pageSize);
            return Ok(ApiResponseBuilder.BuildResponse("User posts retrieved successfully!", posts));
        }

        [HttpGet("category/{categoryId:int}")]
        public async Task<IActionResult> GetPostsByCategoryId(int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var posts = await postService.GetPostsByCategoryIdAsync(categoryId, page, pageSize);
            return Ok(ApiResponseBuilder.BuildResponse("Category posts retrieved successfully!", posts));
        }

        [HttpGet("location")]
        public async Task<IActionResult> GetPostsByLocation([FromQuery] int provinceCode, [FromQuery] int? districtCode = null, [FromQuery] int? wardCode = null)
        {
            var posts = await postService.GetPostsByLocationAsync(provinceCode, districtCode, wardCode);
            return Ok(ApiResponseBuilder.BuildResponse("Location posts retrieved successfully!", posts));
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetPostsByStatus(PostStatus status)
        {
            var posts = await postService.GetPostsByStatusAsync(status);
            return Ok(ApiResponseBuilder.BuildResponse($"Posts with status {status} retrieved successfully!", posts));
        }

        [HttpGet("moderation")]
        [Authorize(Roles = "ALOHA_ADMIN")]
        public async Task<IActionResult> GetPostsForModeration()
        {
            var posts = await postService.GetPostsForModerationAsync();
            return Ok(ApiResponseBuilder.BuildResponse("Posts for moderation retrieved successfully!", posts));
        }

        [HttpPost]
        [ValidateModel]
        //[Authorize]
        public async Task<IActionResult> CreatePost([FromBody] PostCreateRequest request)
        {
            var post = await postService.CreatePostAsync(request);
            return CreatedAtAction(nameof(GetPostById), new { postId = post.Id },
                ApiResponseBuilder.BuildResponse("Post created successfully!", post));
        }

        [HttpPut("{postId:guid}")]
        [ValidateModel]
        [Authorize]
        public async Task<IActionResult> UpdatePost(Guid postId, [FromBody] PostUpdateRequest request)
        {
            var post = await postService.UpdatePostAsync(postId, request);
            return Ok(ApiResponseBuilder.BuildResponse("Post updated successfully!", post));
        }

        [HttpPut("{postId:guid}/status")]
        [Authorize(Roles = "ALOHA_ADMIN")]
        public async Task<IActionResult> UpdatePostStatus(Guid postId, [FromBody] PostStatus status)
        {
            var post = await postService.UpdatePostStatusAsync(postId, status);
            if (post == null)
                return NotFound(ApiResponseBuilder.BuildResponse<object>("Post not found", null));

            return Ok(ApiResponseBuilder.BuildResponse("Post status updated successfully!", post));
        }

        [HttpPut("{postId:guid}/activate")]
        [Authorize]
        public async Task<IActionResult> ActivatePost(Guid postId, [FromBody] bool isActive)
        {
            var post = await postService.ActivatePostAsync(postId, isActive);
            if (post == null)
                return NotFound(ApiResponseBuilder.BuildResponse<object>("Post not found", null));

            return Ok(ApiResponseBuilder.BuildResponse($"Post {(isActive ? "activated" : "deactivated")} successfully!", post));
        }

        [HttpPut("{postId:guid}/push")]
        [Authorize]
        public async Task<IActionResult> PushPost(Guid postId)
        {
            var post = await postService.PushPostAsync(postId);
            if (post == null)
                return NotFound(ApiResponseBuilder.BuildResponse<object>("Post not found", null));

            return Ok(ApiResponseBuilder.BuildResponse("Post pushed successfully!", post));
        }

        [HttpDelete("{postId:guid}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(Guid postId)
        {
            var result = await postService.DeletePostAsync(postId);
            if (!result)
                return NotFound(ApiResponseBuilder.BuildResponse<object>("Post not found", null));

            return Ok(ApiResponseBuilder.BuildResponse<object>("Post deleted successfully!", null));
        }

        [HttpHead("{postId:guid}")]
        public async Task<IActionResult> CheckPostExists(Guid postId)
        {
            var exists = await postService.PostExistsAsync(postId);
            return exists ? Ok() : NotFound();
        }
    }
}
