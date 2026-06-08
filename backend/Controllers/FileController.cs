using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrainingService.Services;

namespace TrainingService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileController : ControllerBase
{
    private readonly IFileService _files;

    public FileController(IFileService files) => _files = files;

    private string Username => User.FindFirstValue(ClaimTypes.Name) ?? "unknown";

    /// <summary>Upload a file and get back its GUID (store this as AttachRecordId on Course/Section)</summary>
    [HttpPost("upload")]
    [RequestSizeLimit(52_428_800)] // 50 MB
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { isValid = false, message = "No file provided" });

        try
        {
            var record = await _files.UploadAsync(file, Username);
            return Ok(new
            {
                isValid = true,
                id = record.Id,
                name = record.OriginalName,
                contentType = record.ContentType,
                size = record.Size,
                url = $"/api/File/{record.Id}"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { isValid = false, message = ex.Message });
        }
    }

    /// <summary>Download / stream a file by its GUID</summary>
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> Download(Guid id)
    {
        var result = await _files.DownloadAsync(id);
        if (result == null) return NotFound();

        var (data, contentType, fileName) = result.Value;

        // Inline for browser-viewable types (PDF, images), attachment for others
        var inline = contentType.StartsWith("image/") || contentType == "application/pdf"
                  || contentType.StartsWith("video/") || contentType == "text/plain";

        var disposition = inline
            ? $"inline; filename=\"{fileName}\""
            : $"attachment; filename=\"{fileName}\"";

        Response.Headers["Content-Disposition"] = disposition;
        return File(data, contentType);
    }

    /// <summary>Get file metadata without downloading</summary>
    [AllowAnonymous]
    [HttpGet("{id}/info")]
    public async Task<IActionResult> Info(Guid id)
    {
        var record = await _files.GetInfoAsync(id);
        if (record == null) return NotFound();

        return Ok(new
        {
            id = record.Id,
            name = record.OriginalName,
            contentType = record.ContentType,
            size = record.Size,
            uploadedAt = record.UploadedAt,
            url = $"/api/File/{record.Id}"
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _files.DeleteAsync(id);
        return deleted ? Ok(new { isValid = true }) : NotFound();
    }
}
