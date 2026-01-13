using EventeApi.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventeApi.Infrastructure.Services;

/// <summary>
/// Local file system image upload service
/// </summary>
public class ImageUploadService : IImageUploadService
{
    private readonly ILogger<ImageUploadService> _logger;
    private readonly IConfiguration _configuration;
    private readonly long _maxFileSizeBytes;
    private readonly string _uploadPath;
    
    // Allowed image MIME types
    private static readonly Dictionary<string, string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "image/jpeg", ".jpg" },
        { "image/jpg", ".jpg" },
        { "image/png", ".png" },
        { "image/gif", ".gif" },
        { "image/webp", ".webp" }
    };

    // Allowed file extensions
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp"
    };

    // File signature (magic bytes) validation
    private static readonly Dictionary<string, byte[][]> FileSignatures = new()
    {
        { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
        { ".gif", new[] { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
        { ".webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } }
    };

    public ImageUploadService(ILogger<ImageUploadService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Read configuration values with defaults
        var uploadSection = _configuration.GetSection("ImageUpload");
        
        // Parse max file size
        var maxFileSizeStr = uploadSection["MaxFileSizeBytes"];
        _maxFileSizeBytes = !string.IsNullOrEmpty(maxFileSizeStr) && long.TryParse(maxFileSizeStr, out var parsedSize) 
            ? parsedSize 
            : 5L * 1024 * 1024; // Default 5MB
            
        // Get upload path
        _uploadPath = uploadSection["UploadPath"] ?? "wwwroot/uploads";
    }

    public async Task<ImageUploadResult> UploadImageAsync(Stream fileStream, string fileName, string contentType, string category = "events")
    {
        try
        {
            // Validate the image
            var validation = ValidateImage(fileName, contentType, fileStream.Length);
            if (!validation.IsValid)
            {
                return new ImageUploadResult(false, null, null, 0, validation.ErrorMessage);
            }

            // Validate file signature (magic bytes)
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!await ValidateFileSignatureAsync(fileStream, extension))
            {
                return new ImageUploadResult(false, null, null, 0, "Invalid file content. File does not match its extension.");
            }

            // Reset stream position after validation
            fileStream.Position = 0;

            // Generate secure filename
            var secureFileName = GenerateSecureFileName(extension);
            
            // Create category directory
            var categoryPath = Path.Combine(_uploadPath, category);
            Directory.CreateDirectory(categoryPath);

            // Full file path
            var filePath = Path.Combine(categoryPath, secureFileName);

            // Save the file
            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            // Generate URL (relative path for serving)
            var url = $"/uploads/{category}/{secureFileName}";

            _logger.LogInformation("Image uploaded successfully: {FileName} -> {Url}", fileName, url);

            return new ImageUploadResult(true, url, secureFileName, fileStream.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image: {FileName}", fileName);
            return new ImageUploadResult(false, null, null, 0, "An error occurred while uploading the image.");
        }
    }

    public Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return Task.FromResult(false);
            }

            // Convert URL to file path
            var relativePath = imageUrl.TrimStart('/').Replace("uploads/", "");
            var filePath = Path.Combine(_uploadPath, relativePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Image deleted successfully: {FilePath}", filePath);
                return Task.FromResult(true);
            }

            _logger.LogWarning("Image not found for deletion: {FilePath}", filePath);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {ImageUrl}", imageUrl);
            return Task.FromResult(false);
        }
    }

    public ImageValidationResult ValidateImage(string fileName, string contentType, long fileSize)
    {
        // Validate file size
        if (fileSize <= 0)
        {
            return new ImageValidationResult(false, "File is empty.");
        }

        if (fileSize > _maxFileSizeBytes)
        {
            var maxSizeMB = _maxFileSizeBytes / (1024 * 1024);
            return new ImageValidationResult(false, $"File size exceeds maximum allowed size of {maxSizeMB}MB.");
        }

        // Validate MIME type
        if (string.IsNullOrWhiteSpace(contentType) || !AllowedMimeTypes.ContainsKey(contentType))
        {
            return new ImageValidationResult(false, "Invalid file type. Allowed types: JPG, PNG, GIF, WebP.");
        }

        // Validate file extension
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return new ImageValidationResult(false, "Invalid file extension. Allowed extensions: .jpg, .jpeg, .png, .gif, .webp");
        }

        // Validate that MIME type matches extension
        var expectedExtensions = new[] { AllowedMimeTypes[contentType], ".jpeg" }; // Handle .jpg/.jpeg
        if (!expectedExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase)) &&
            !(contentType == "image/jpeg" && (extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))))
        {
            return new ImageValidationResult(false, "File extension does not match content type.");
        }

        return new ImageValidationResult(true);
    }

    private static string GenerateSecureFileName(string extension)
    {
        // Generate a GUID-based filename to prevent overwrites and path traversal attacks
        return $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
    }

    private static async Task<bool> ValidateFileSignatureAsync(Stream fileStream, string extension)
    {
        if (!FileSignatures.TryGetValue(extension, out var signatures))
        {
            return true; // No signature check available for this type
        }

        // Get the maximum signature length to read
        var maxSignatureLength = signatures.Max(s => s.Length);
        var headerBytes = new byte[maxSignatureLength];

        fileStream.Position = 0;
        var bytesRead = await fileStream.ReadAsync(headerBytes);

        if (bytesRead < signatures.Min(s => s.Length))
        {
            return false; // File too small
        }

        // Check if any signature matches
        foreach (var signature in signatures)
        {
            if (headerBytes.Take(signature.Length).SequenceEqual(signature))
            {
                return true;
            }
        }

        return false;
    }
}
