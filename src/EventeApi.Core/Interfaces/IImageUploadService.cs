namespace EventeApi.Core.Interfaces;

/// <summary>
/// Service for handling image upload operations
/// </summary>
public interface IImageUploadService
{
    /// <summary>
    /// Uploads an image file and returns the URL
    /// </summary>
    /// <param name="fileStream">The image file stream</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <param name="category">Category for organizing uploads (e.g., "events", "badges")</param>
    /// <returns>Upload result with URL and metadata</returns>
    Task<ImageUploadResult> UploadImageAsync(Stream fileStream, string fileName, string contentType, string category = "events");

    /// <summary>
    /// Deletes an image by its URL or filename
    /// </summary>
    /// <param name="imageUrl">The URL or path of the image to delete</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteImageAsync(string imageUrl);

    /// <summary>
    /// Validates if the file is a valid image
    /// </summary>
    /// <param name="fileName">File name with extension</param>
    /// <param name="contentType">MIME type</param>
    /// <param name="fileSize">File size in bytes</param>
    /// <returns>Validation result</returns>
    ImageValidationResult ValidateImage(string fileName, string contentType, long fileSize);
}

/// <summary>
/// Result of an image upload operation
/// </summary>
public record ImageUploadResult(
    bool Success,
    string? Url,
    string? FileName,
    long FileSize,
    string? ErrorMessage = null
);

/// <summary>
/// Result of image validation
/// </summary>
public record ImageValidationResult(
    bool IsValid,
    string? ErrorMessage = null
);
