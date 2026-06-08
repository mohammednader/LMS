using TrainingService.Models;

namespace TrainingService.Services;

public interface IFileService
{
    Task<FileRecord> UploadAsync(IFormFile file, string uploadedBy);
    Task<(byte[] Data, string ContentType, string FileName)?> DownloadAsync(Guid id);
    Task<FileRecord?> GetInfoAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}
