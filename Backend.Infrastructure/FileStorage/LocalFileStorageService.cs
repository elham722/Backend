using Backend.Application.Common.Interfaces.Infrastructure;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.FileStorage;

/// <summary>
/// Local file storage service implementation
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly LocalFileStorageOptions _options;

    public LocalFileStorageService(IOptions<LocalFileStorageOptions> options)
    {
        _options = options.Value;
        EnsureDirectoryExists(_options.BasePath);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string? folder = null)
    {
        var filePath = GetFilePath(fileName, folder);
        var directory = Path.GetDirectoryName(filePath);
        
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var fileStream2 = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStream2);

        return filePath;
    }

    public async Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType, string? folder = null)
    {
        var filePath = GetFilePath(fileName, folder);
        var directory = Path.GetDirectoryName(filePath);
        
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(filePath, fileBytes);
        return filePath;
    }

    public async Task<Stream> DownloadFileAsync(string filePath)
    {
        if (!await FileExistsAsync(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    public async Task<byte[]> DownloadFileAsBytesAsync(string filePath)
    {
        if (!await FileExistsAsync(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        return await File.ReadAllBytesAsync(filePath);
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        if (!await FileExistsAsync(filePath))
        {
            return false;
        }

        File.Delete(filePath);
        return true;
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        return await Task.FromResult(File.Exists(filePath));
    }

    public async Task<Application.Common.Interfaces.Infrastructure.FileInfo> GetFileInfoAsync(string filePath)
    {
        if (!await FileExistsAsync(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var systemFileInfo = new System.IO.FileInfo(filePath);
        return new Application.Common.Interfaces.Infrastructure.FileInfo
        {
            Name = systemFileInfo.Name,
            Path = filePath,
            Size = systemFileInfo.Length,
            ContentType = GetContentType(systemFileInfo.Extension),
            CreatedAt = systemFileInfo.CreationTimeUtc,
            ModifiedAt = systemFileInfo.LastWriteTimeUtc
        };
    }

    public async Task<string> GetTemporaryUrlAsync(string filePath, int expirationMinutes = 60)
    {
        // For local storage, we return the file path
        // In a real implementation, you might generate a temporary URL
        return await Task.FromResult(filePath);
    }

    public async Task<IEnumerable<string>> ListFilesAsync(string folder, string? searchPattern = null)
    {
        var folderPath = Path.Combine(_options.BasePath, folder);
        
        if (!Directory.Exists(folderPath))
        {
            return await Task.FromResult(Enumerable.Empty<string>());
        }

        var files = Directory.GetFiles(folderPath, searchPattern ?? "*");
        return await Task.FromResult(files);
    }

    private string GetFilePath(string fileName, string? folder)
    {
        var sanitizedFileName = SanitizeFileName(fileName);
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var fileNameWithTimestamp = $"{timestamp}_{sanitizedFileName}";
        
        if (string.IsNullOrEmpty(folder))
        {
            return Path.Combine(_options.BasePath, fileNameWithTimestamp);
        }

        return Path.Combine(_options.BasePath, folder, fileNameWithTimestamp);
    }

    private string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }

    private string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".doc" or ".docx" => "application/msword",
            ".xls" or ".xlsx" => "application/vnd.ms-excel",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}

/// <summary>
/// Local file storage options
/// </summary>
public class LocalFileStorageOptions
{
    public string BasePath { get; set; } = "wwwroot/uploads";
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10MB
    public string[] AllowedExtensions { get; set; } = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt" };
} 