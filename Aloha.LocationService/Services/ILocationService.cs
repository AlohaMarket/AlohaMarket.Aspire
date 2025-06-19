using Aloha.LocationService.Models.Responses;

namespace Aloha.LocationService.Services
{
    public interface ILocationService
    {
        Task<IEnumerable<ProvinceResponse>> GetAllProvincesAsync();
        Task<ProvinceResponse> GetProvinceByCodeAsync(int code);
        Task<bool> IsValidLocationPath(int provinceCode, int districtCode, int wardCode);
        Task<LocationValidationResult> ValidateLocationPathWithText(int provinceCode, int districtCode, int wardCode);
    }
}
