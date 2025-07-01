using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Size = SixLabors.ImageSharp.Size;

namespace Aloha.ServiceDefaults.Cloudinary
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public CloudinaryService(IOptions<CloudinarySettings> options, ILogger<CloudinaryService> logger)
        {
            var settings = options.Value;
            var account = new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret);
            _cloudinary = new CloudinaryDotNet.Cloudinary(account);
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Attempted to upload null or empty file");
                    return string.Empty;
                }

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Transformation = new Transformation()
                        .Quality("auto:good")
                        .FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult?.Error != null)
                {
                    _logger.LogError("Cloudinary upload error: {Error}", uploadResult.Error.Message);
                    throw new InvalidOperationException($"Image upload failed: {uploadResult.Error.Message}");
                }

                return uploadResult?.SecureUrl?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image {FileName}", file?.FileName);
                throw new InvalidOperationException($"Failed to upload image: {ex.Message}", ex);
            }
        }

        public async Task<List<string>> UploadImagesAsync(List<IFormFile> files)
        {
            var urls = new List<string>();
            var errors = new List<string>();

            if (files == null || !files.Any())
            {
                _logger.LogInformation("No files provided for upload");
                return urls;
            }

            foreach (var file in files)
            {
                try
                {
                    var url = await UploadImageAsync(file);
                    if (!string.IsNullOrEmpty(url))
                    {
                        urls.Add(url);
                        _logger.LogDebug("Successfully uploaded image: {FileName}", file.FileName);
                    }
                    else
                    {
                        errors.Add($"Failed to get URL for {file.FileName}");
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Failed to upload {file.FileName}: {ex.Message}";
                    errors.Add(errorMsg);
                    _logger.LogError(ex, "Error uploading individual image: {FileName}", file.FileName);

                    // Option 1: Continue with other files (partial success)
                    // continue;

                    // Option 2: Fail fast (all or nothing)
                    throw new InvalidOperationException($"Image upload failed. {errorMsg}");
                }
            }

            if (errors.Any() && urls.Count == 0)
            {
                throw new InvalidOperationException($"All image uploads failed: {string.Join(", ", errors)}");
            }

            if (errors.Any())
            {
                _logger.LogWarning("Some images failed to upload: {Errors}", string.Join(", ", errors));
            }

            return urls;
        }

        public async Task<string> UploadOptimizedImageAsync(IFormFile file, int maxWidth = 800, int maxHeight = 600)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return string.Empty;
                }

                using var image = await Image.LoadAsync(file.OpenReadStream());

                // Resize if needed
                if (image.Width > maxWidth || image.Height > maxHeight)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(maxWidth, maxHeight),
                        Mode = ResizeMode.Max
                    }));
                }

                using var outputStream = new MemoryStream();
                await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 85 });
                outputStream.Position = 0;

                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription($"optimized_{file.FileName}", outputStream),
                    Transformation = new Transformation()
                        .Quality("auto:good")
                        .FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult?.Error != null)
                {
                    throw new InvalidOperationException($"Optimized image upload failed: {uploadResult.Error.Message}");
                }

                return uploadResult?.SecureUrl?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading optimized image {FileName}", file?.FileName);
                throw new InvalidOperationException($"Failed to upload optimized image: {ex.Message}", ex);
            }
        }

        public async Task<List<string>> UploadOptimizedImagesAsync(List<IFormFile> files, int maxWidth = 800, int maxHeight = 600)
        {
            var urls = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var url = await UploadOptimizedImageAsync(file, maxWidth, maxHeight);
                    if (!string.IsNullOrEmpty(url))
                    {
                        urls.Add(url);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload optimized image: {FileName}", file.FileName);
                    throw; // Re-throw to stop the process
                }
            }

            return urls;
        }
    }
}
