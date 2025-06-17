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

        public async Task<bool> IsValidLocationPath(List<int> locationPath)
        {
            return true;
        }
    }
}
