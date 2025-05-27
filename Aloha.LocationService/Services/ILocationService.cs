using Aloha.LocationService.Models.Responses;

namespace Aloha.LocationService.Services
{
    public interface ILocationService
    {
        Task<IEnumerable<ProvinceResponse>> GetAllProvincesAsync();
        Task<ProvinceResponse> GetProvinceByCodeAsync(int code);
        //Task<IEnumerable<District>> GetDistrictsByProvinceCodeAsync(int provinceCode);
        //Task<IEnumerable<Ward>> GetWardsByDistrictCodeAsync(int districtCode);
    }
}
