using Microsoft.EntityFrameworkCore;
using TrainingService.Data;
using TrainingService.DTOs;
using TrainingService.Models;
using TrainingService.Models.Shared;

namespace TrainingService.Services;

public class TrainingServiceImpl : ITrainingService
{
    private readonly TrainingDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<TrainingServiceImpl> _logger;

    public TrainingServiceImpl(TrainingDbContext db, IConfiguration config, ILogger<TrainingServiceImpl> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    // Base URL used to build materialUrl in responses (e.g. "http://localhost:44379")
    private string ApiBase => _config["App:BaseUrl"]?.TrimEnd('/') ?? string.Empty;

    // OrgId normalization: multiple org IDs map to the same physical org
    private static int NormalizeOrgId(int orgId) => orgId switch
    {
        1 or 4 or 9 => 1,
        2 or 5 or 10 => 2,
        _ => orgId
    };

    public async Task<BaseResponse> AddUpdateTrainingCourse(string userName, TrainingCourseDTO model)
    {
        var response = new BaseResponse();
        try
        {
            var existing = model.id > 0
                ? await _db.TrainingCourses.FindAsync(model.id)
                : null;

            if (existing == null)
            {
                var entity = model.ToEntity();
                entity.UserId = userName;
                entity.Creatd = DateTime.Now;
                _db.TrainingCourses.Add(entity);
                await _db.SaveChangesAsync();
                response.IsValid = true;
                response.Id = entity.id;
                response.Message = "Course added successfully";
                response.ArMessage = "تمت إضافة الدورة بنجاح";
            }
            else
            {
                existing.Name = model.Name;
                existing.NameAr = model.NameAr;
                existing.IsActive = model.IsActive;
                existing.TargetedAudiance = model.TargetedAudiance;
                existing.AttachRecordId = model.AttachRecordId;
                existing.SurveyId = model.SurveyId;
                await _db.SaveChangesAsync();
                response.IsValid = true;
                response.Id = existing.id;
                response.Message = "Course updated successfully";
                response.ArMessage = "تم تحديث الدورة بنجاح";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding/updating course");
            response.IsValid = false;
            response.Message = "Error saving course";
            response.ArMessage = "خطأ في حفظ الدورة";
        }
        return response;
    }

    public async Task<BaseResponse> AddUpdateCourseSection(string userName, CourseSectionDto model)
    {
        var response = new BaseResponse();
        try
        {
            var existing = model.id > 0
                ? await _db.CourseSections.FindAsync(model.id)
                : null;

            if (existing == null)
            {
                var entity = new CourseSection
                {
                    TrainingCourseId = model.CourseId, NameAr = model.NameAr, Name = model.Name,
                    Description = model.Description, IsActive = model.IsActive,
                    OrganizationId = model.OrganizationId, UserId = userName,
                    AttachRecordId = model.AttachRecordId, ExternalUrl = model.ExternalUrl,
                    Creatd = DateTime.Now
                };
                _db.CourseSections.Add(entity);
                await _db.SaveChangesAsync();
                response.IsValid = true;
                response.Id = entity.id;
                response.Message = "Section added";
                response.ArMessage = "تمت إضافة القسم";
            }
            else
            {
                existing.Name = model.Name;
                existing.NameAr = model.NameAr;
                existing.Description = model.Description;
                existing.IsActive = model.IsActive;
                existing.AttachRecordId = model.AttachRecordId;
                existing.ExternalUrl = model.ExternalUrl;
                await _db.SaveChangesAsync();
                response.IsValid = true;
                response.Id = existing.id;
                response.Message = "Section updated";
                response.ArMessage = "تم تحديث القسم";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving section");
            response.IsValid = false;
            response.Message = "Error saving section";
        }
        return response;
    }

    public async Task<TrainingCoursesResponse> GetTrainingCourses(int orgId = 0, string username = "", int courseId = 0, bool fullAccess = false)
    {
        var response = new TrainingCoursesResponse();
        try
        {
            var normalized = NormalizeOrgId(orgId);
            var query = _db.TrainingCourses.Where(c => c.IsActive);

            if (!fullAccess && !string.IsNullOrEmpty(username))
                query = query.Where(c => c.UserId == username || c.OrganizationId == normalized);
            else if (orgId > 0)
                query = query.Where(c => NormalizeOrgId(c.OrganizationId) == normalized);

            if (courseId > 0)
                query = query.Where(c => c.id == courseId);

            var courses = await query.ToListAsync();
            response.Courses = courses.Select(TrainingCourseDTO.FromEntity).ToList();

            // Populate course URLs from config
            var baseUrl = _config["InPortal:BaseUrl"];
            var surveyBaseUrl = _config["ExPortal:BaseUrl"];
            foreach (var c in response.Courses)
            {
                c.CourseURL = $"{baseUrl}/training/course/{c.id}";
                if (c.SurveyId.HasValue)
                    c.SurveyUrl = $"{surveyBaseUrl}/survey/{c.SurveyId}";
            }

            if (courseId > 0 && courses.Any())
            {
                var sections = await _db.CourseSections
                    .Where(s => s.TrainingCourseId == courseId && s.IsActive)
                    .ToListAsync();
                response.CourseSections = sections.Select(s => CourseSectionDto.FromEntity(s, ApiBase)).ToList();
            }

            response.isEditable = fullAccess;
            response.IsValid = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting courses");
            response.IsValid = false;
            response.Message = "Error retrieving courses";
        }
        return response;
    }

    public async Task<BaseResponse> DeleteSection(int id)
    {
        var s = await _db.CourseSections.FindAsync(id);
        if (s == null) return new BaseResponse { IsValid = false, Message = "Section not found" };
        s.IsActive = false;
        await _db.SaveChangesAsync();
        return new BaseResponse { IsValid = true, Message = "Section deleted" };
    }

    public async Task<CourseSectionResponse> GetCourseSections(int orgId = 0, string username = "", int courseId = 0)
    {
        var response = new CourseSectionResponse();
        try
        {
            var query = _db.CourseSections.Where(s => s.IsActive);
            if (courseId > 0) query = query.Where(s => s.TrainingCourseId == courseId);

            var sections = await query.ToListAsync();
            response.CourseSections = sections.Select(s => CourseSectionDto.FromEntity(s, ApiBase)).ToList();
            response.IsValid = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sections");
            response.IsValid = false;
            response.Message = "Error retrieving sections";
        }
        return response;
    }
}
