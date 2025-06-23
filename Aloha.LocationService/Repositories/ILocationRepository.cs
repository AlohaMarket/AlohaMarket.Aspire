using Aloha.LocationService.Models.Entities;

namespace Aloha.LocationService.Repositories
{
    public interface ILocationRepository
    {
        Task<IEnumerable<Province>> GetAllAsync();
        Task<Province> GetByCodeAsync(int code);
        Task<Province> GetByCodenameAsync(string codename);
        Task<IEnumerable<Province>> SearchAsync(string keyword);
    }
}
