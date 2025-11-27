using System.IO;

namespace RazorEcom.Services
{
    public class ImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IWebHostEnvironment environment, ILogger<ImageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        /// <summary>
        /// Upload image file and save to wwwroot/images/products/
        /// </summary>
        /// <param name="file">Image file from form upload</param>
        /// <returns>Relative path to the saved image (e.g., /images/products/filename.jpg) or null if failed</returns>
        public async Task<string?> UploadImageAsync(IFormFile file)
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Upload attempted with null or empty file");
                    return null;
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    _logger.LogWarning("File size exceeds 5MB limit: {Size}", file.Length);
                    return null;
                }

                // Validate file extension
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(extension))
                {
                    _logger.LogWarning("Invalid file extension: {Extension}", extension);
                    return null;
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                
                // Create directory if it doesn't exist
                var uploadPath = Path.Combine(_environment.WebRootPath, "images", "products");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Full file path
                var filePath = Path.Combine(uploadPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("Image uploaded successfully: {FileName}", fileName);

                // Return relative path for storing in database
                return $"/images/products/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image");
                return null;
            }
        }

        /// <summary>
        /// Delete image file from wwwroot/images/products/
        /// </summary>
        /// <param name="imagePath">Relative path (e.g., /images/products/filename.jpg)</param>
        /// <returns>True if deleted successfully</returns>
        public bool DeleteImage(string? imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return false;

                // Only delete files in our products folder
                if (!imagePath.StartsWith("/images/products/"))
                    return false;

                var fileName = Path.GetFileName(imagePath);
                var filePath = Path.Combine(_environment.WebRootPath, "images", "products", fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Image deleted: {FileName}", fileName);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image: {ImagePath}", imagePath);
                return false;
            }
        }
    }
}
