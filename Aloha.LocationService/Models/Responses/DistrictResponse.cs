namespace Aloha.LocationService.Models.Responses
{
    public record DistrictResponse(
        string Name,
        int Code,
        List<WardResponse> Wards
    );
}
