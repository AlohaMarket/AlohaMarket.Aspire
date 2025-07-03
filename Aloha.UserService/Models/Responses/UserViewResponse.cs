namespace Aloha.MicroService.User.Models.Responses
{
    public record UserViewResponse(
        Guid Id,
        string UserName,
        string? AvatarUrl,
        string? PhoneNumber,
        DateTime CreatedAt
        );
}
