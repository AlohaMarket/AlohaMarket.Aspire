namespace Aloha.CategoryService.Models.Responses
{
    public record ViewCategoryResponse(
    int Id,
    string Name,
    string DisplayName,
    string Description,
    int ParentId,
    int Level,
    int SortOrder,
    ICollection<ViewCategoryResponse> Children
    );
}
