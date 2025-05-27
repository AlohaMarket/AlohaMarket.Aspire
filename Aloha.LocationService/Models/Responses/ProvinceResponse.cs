using Aloha.LocationService.Models.Entities;

namespace Aloha.LocationService.Models.Responses
{
    public record ProvinceResponse(
        string Name,            // "Thành phố Hà Nội"  
        int Code,               // 1, 2, 4, 6, …  
        string DivisionType,    // "tỉnh", "thành phố trung ương"  
        string Codename,     // "thanh_pho_ha_noi",
        List<District> Districts  // ["Quận Hoàn Kiếm", "Quận Đống Đa", ...]
    );
}
