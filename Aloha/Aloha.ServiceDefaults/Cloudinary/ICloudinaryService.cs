using Microsoft.AspNetCore.Http;

namespace Aloha.ServiceDefaults.Cloudinary
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<List<string>> UploadImagesAsync(List<IFormFile> files);
    }
}
