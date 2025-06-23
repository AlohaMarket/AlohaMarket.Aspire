using Aloha.LocationService.Models.Responses;
using Aloha.LocationService.Repositories;
using Aloha.Shared.Exceptions;
using AutoMapper;

namespace Aloha.LocationService.Services
{
    public class LocationService(ILocationRepository repo, IMapper mapper) : ILocationService
    {
        public async Task<IEnumerable<ProvinceResponse>> GetAllProvincesAsync()
        {
            var provinces = await repo.GetAllAsync();
            return mapper.Map<IEnumerable<ProvinceResponse>>(provinces);
        }

        public async Task<ProvinceResponse> GetProvinceByCodeAsync(int code)
        {
            var province = await repo.GetByCodeAsync(code);
            if (province == null)
            {
                throw new NotFoundException($"Province with code {code} not found.");
            }
            return mapper.Map<ProvinceResponse>(province);
        }

        public async Task<bool> IsValidLocationPath(int provinceCode, int districtCode, int wardCode)
        {
            var province = await repo.GetByCodeAsync(provinceCode);
            if (province == null)
                return false;

            var district = province.Districts.FirstOrDefault(d => d.Code == districtCode);
            if (district == null)
                return false;
            var ward = district.Wards.FirstOrDefault(w => w.Code == wardCode);
            if (ward == null)
                return false;

            return true;
        }

        public async Task<LocationValidationResult> ValidateLocationPathWithText(int provinceCode, int districtCode, int wardCode)
        {
            var result = new LocationValidationResult { IsValid = false };

            var province = await repo.GetByCodeAsync(provinceCode);
            if (province == null)
                return result;

            var district = province.Districts.FirstOrDefault(d => d.Code == districtCode);
            if (district == null)
                return result;

            var ward = district.Wards.FirstOrDefault(w => w.Code == wardCode);
            if (ward == null)
                return result;

            result.IsValid = true;
            result.ProvinceText = province.Name;
            result.DistrictText = district.Name;
            result.WardText = ward.Name;

            return result;
        }
    }
}
