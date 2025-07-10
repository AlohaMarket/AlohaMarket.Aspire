using Microsoft.AspNetCore.Http;

namespace Aloha.ServiceDefaults.Cloudinary
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<List<string>> UploadImagesAsync(List<IFormFile> files);

        Task<string> UploadOptimizedImageAsync(IFormFile file, int maxWidth = 800, int maxHeight = 600);

        Task<List<string>> UploadOptimizedImagesAsync(List<IFormFile> files, int maxWidth = 800, int maxHeight = 600);
    }
}
