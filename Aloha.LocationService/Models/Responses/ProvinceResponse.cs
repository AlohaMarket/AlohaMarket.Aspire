namespace Aloha.LocationService.Models.Responses
{
    public record ProvinceResponse(
        string Name,            // "Thành phố Hà Nội"  
        int Code,               // 1, 2, 4, 6, …  
        List<DistrictResponse> Districts  // ["Quận Hoàn Kiếm", "Quận Đống Đa", ...]
    );
}
