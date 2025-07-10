using Aloha.Shared.Meta;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Aloha.Shared.Extensions
{
    public class ValidateFileAttribute : ActionFilterAttribute
    {
        private readonly int _maxSizeInMB;
        private readonly string[] _allowedExtensions;
        private readonly bool _allowMultipleFiles;
        private readonly string _formFieldName;

        public ValidateFileAttribute(
            int maxSizeInMB = 5,
            string[]? allowedExtensions = null,
            bool allowMultipleFiles = false,
            string formFieldName = "file")
        {
            _maxSizeInMB = maxSizeInMB;
            _allowedExtensions = allowedExtensions ?? new[] { ".jpg", ".jpeg", ".png" };
            _allowMultipleFiles = allowMultipleFiles;
            _formFieldName = formFieldName;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var files = context.HttpContext.Request.Form.Files
                .Where(f => f.Name.Equals(_formFieldName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!files.Any())
            {
                AddError(context, "No file was uploaded");
                return;
            }

            if (!_allowMultipleFiles && files.Count > 1)
            {
                AddError(context, "Multiple files are not allowed");
                return;
            }

            foreach (var file in files)
            {
                var validationError = ValidateFile(file);
                if (!string.IsNullOrEmpty(validationError))
                {
                    AddError(context, validationError);
                    return;
                }
            }
        }

        private string? ValidateFile(IFormFile file)
        {
            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return $"Unsupported file extension '{extension}'. Only {String.Join(", ", _allowedExtensions)} are allowed";
            }

            // Validate file size
            if (file.Length > _maxSizeInMB * 1024 * 1024)
            {
                return $"The file '{file.FileName}' exceeds the maximum size of {_maxSizeInMB}MB";
            }

            return null;
        }

        private void AddError(ActionExecutingContext context, string errorMessage)
        {
            var response = ApiResponseBuilder.BuildResponse(
                data: new { field = _formFieldName, error = errorMessage },
                message: "File validation failed"
            );

            context.Result = new ObjectResult(response)
            {
                StatusCode = StatusCodes.Status422UnprocessableEntity
            };
        }
    }
}
