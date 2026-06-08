using Microsoft.EntityFrameworkCore;
using TrainingService.Data;
using TrainingService.Models;

namespace TrainingService.Services;

public class FileServiceImpl : IFileService
{
    private readonly TrainingDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileServiceImpl> _logger;

    private static readonly HashSet<string> _allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf", "image/jpeg", "image/png", "image/gif", "image/webp",
        "video/mp4", "video/webm",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain"
    };

    public FileServiceImpl(TrainingDbContext db, IWebHostEnvironment env, ILogger<FileServiceImpl> logger)
    {
        _db = db;
        _env = env;
        _logger = logger;
    }

    private string UploadsPath => Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");

    public async Task<FileRecord> UploadAsync(IFormFile file, string uploadedBy)
    {
        if (!_allowed.Contains(file.ContentType))
            throw new InvalidOperationException($"File type '{file.ContentType}' is not allowed.");

        if (file.Length > 50 * 1024 * 1024) // 50 MB
            throw new InvalidOperationException("File size exceeds 50 MB limit.");

        Directory.CreateDirectory(UploadsPath);

        var record = new FileRecord
        {
            OriginalName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            Size = file.Length,
            UploadedBy = uploadedBy,
            UploadedAt = DateTime.Now
        };

        var ext = Path.GetExtension(file.FileName);
        var stored = $"{record.Id}{ext}";
        record.StoredPath = Path.Combine(UploadsPath, stored);

        await using var stream = File.Create(record.StoredPath);
        await file.CopyToAsync(stream);

        _db.FileRecords.Add(record);
        await _db.SaveChangesAsync();

        _logger.LogInformation("File uploaded: {Name} → {Id}", record.OriginalName, record.Id);
        return record;
    }

    public async Task<(byte[] Data, string ContentType, string FileName)?> DownloadAsync(Guid id)
    {
        var record = await _db.FileRecords.FindAsync(id);
        if (record == null || !File.Exists(record.StoredPath)) return null;

        var data = await File.ReadAllBytesAsync(record.StoredPath);
        return (data, record.ContentType, record.OriginalName);
    }

    public async Task<FileRecord?> GetInfoAsync(Guid id) =>
        await _db.FileRecords.FindAsync(id);

    public async Task<bool> DeleteAsync(Guid id)
    {
        var record = await _db.FileRecords.FindAsync(id);
        if (record == null) return false;

        if (File.Exists(record.StoredPath))
            File.Delete(record.StoredPath);

        _db.FileRecords.Remove(record);
        await _db.SaveChangesAsync();
        return true;
    }
}
