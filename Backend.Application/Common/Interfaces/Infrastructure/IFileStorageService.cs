namespace Backend.Application.Common.Interfaces.Infrastructure;

/// <summary>
/// Interface for file storage services
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage
    /// </summary>
    /// <param name="fileStream">File stream</param>
    /// <param name="fileName">File name</param>
    /// <param name="contentType">Content type</param>
    /// <param name="folder">Folder path (optional)</param>
    /// <returns>File URL or path</returns>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null);

    /// <summary>
    /// Uploads a file from byte array
    /// </summary>
    /// <param name="fileBytes">File bytes</param>
    /// <param name="fileName">File name</param>
    /// <param name="contentType">Content type</param>
    /// <param name="folder">Folder path (optional)</param>
    /// <returns>File URL or path</returns>
    Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType, string? folder = null);

    /// <summary>
    /// Downloads a file from storage
    /// </summary>
    /// <param name="filePath">File path or URL</param>
    /// <returns>File stream</returns>
    Task<Stream> DownloadFileAsync(string filePath);

    /// <summary>
    /// Downloads a file as byte array
    /// </summary>
    /// <param name="filePath">File path or URL</param>
    /// <returns>File bytes</returns>
    Task<byte[]> DownloadFileAsBytesAsync(string filePath);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="filePath">File path or URL</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteFileAsync(string filePath);

    /// <summary>
    /// Checks if a file exists
    /// </summary>
    /// <param name="filePath">File path or URL</param>
    /// <returns>True if file exists</returns>
    Task<bool> FileExistsAsync(string filePath);

    /// <summary>
    /// Gets file information
    /// </summary>
    /// <param name="filePath">File path or URL</param>
    /// <returns>File information</returns>
    Task<FileInfo> GetFileInfoAsync(string filePath);

    /// <summary>
    /// Gets a temporary URL for file access
    /// </summary>
    /// <param name="filePath">File path or URL</param>
    /// <param name="expirationMinutes">Expiration time in minutes</param>
    /// <returns>Temporary URL</returns>
    Task<string> GetTemporaryUrlAsync(string filePath, int expirationMinutes = 60);

    /// <summary>
    /// Lists files in a folder
    /// </summary>
    /// <param name="folder">Folder path</param>
    /// <param name="searchPattern">Search pattern (optional)</param>
    /// <returns>List of file paths</returns>
    Task<IEnumerable<string>> ListFilesAsync(string folder, string? searchPattern = null);
}

/// <summary>
/// File information
/// </summary>
public class FileInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
} 