using TrainingService.Models;
using TrainingService.Models.Shared;

namespace TrainingService.DTOs;

public class TrainingCourseDTO
{
    public int id { get; set; }
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public DateTime Creatd { get; set; }
    public bool IsActive { get; set; }
    public int OrganizationId { get; set; }
    public string? UserId { get; set; }
    public string? TargetedAudiance { get; set; }
    public Guid? AttachRecordId { get; set; }
    public string? CourseURL { get; set; }
    public int? SurveyId { get; set; }
    public string? SurveyUrl { get; set; }

    public static TrainingCourseDTO FromEntity(TrainingCourse e) => new()
    {
        id = e.id, Name = e.Name, NameAr = e.NameAr, Creatd = e.Creatd,
        IsActive = e.IsActive, OrganizationId = e.OrganizationId, UserId = e.UserId,
        TargetedAudiance = e.TargetedAudiance, AttachRecordId = e.AttachRecordId, SurveyId = e.SurveyId
    };

    public TrainingCourse ToEntity() => new()
    {
        id = id, Name = Name, NameAr = NameAr, IsActive = IsActive,
        OrganizationId = OrganizationId, UserId = UserId,
        TargetedAudiance = TargetedAudiance, AttachRecordId = AttachRecordId, SurveyId = SurveyId
    };
}

public class CourseSectionDto
{
    public int id { get; set; }
    public int CourseId { get; set; }
    public string? NameAr { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime Creatd { get; set; }
    public bool IsActive { get; set; }
    public int OrganizationId { get; set; }
    public string? UserId { get; set; }

    // File upload
    public Guid? AttachRecordId { get; set; }
    public string? MaterialUrl { get; set; }      // ← populated in response: /api/File/{id}
    public string? MaterialName { get; set; }     // ← original file name

    // External link (YouTube, SharePoint, etc.)
    public string? ExternalUrl { get; set; }

    public List<QuestionDto>? questions { get; set; }

    public static CourseSectionDto FromEntity(CourseSection e, string? apiBase = null) => new()
    {
        id = e.id, CourseId = e.TrainingCourseId, NameAr = e.NameAr, Name = e.Name,
        Description = e.Description, Creatd = e.Creatd, IsActive = e.IsActive,
        OrganizationId = e.OrganizationId, UserId = e.UserId,
        AttachRecordId = e.AttachRecordId, ExternalUrl = e.ExternalUrl,
        MaterialUrl = e.AttachRecordId.HasValue && !string.IsNullOrEmpty(apiBase)
            ? $"{apiBase}/api/File/{e.AttachRecordId}"
            : null
    };
}

public class TestDto
{
    public int id { get; set; }
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public int TrainingCourseId { get; set; }
    public int OrganizationId { get; set; }
    public string? UserId { get; set; }
    public int PassScore { get; set; }
    public int ExpiryDuration { get; set; }
    public List<QuestionDto>? Questions { get; set; }

    public static TestDto FromEntity(Test e) => new()
    {
        id = e.id, Name = e.Name, IsActive = e.IsActive, TrainingCourseId = e.TrainingCourseId,
        OrganizationId = e.OrganizationId, UserId = e.UserId,
        PassScore = e.PassScore, ExpiryDuration = e.ExpiryDuration
    };

    public Test ToEntity() => new()
    {
        id = id, Name = Name, IsActive = IsActive, TrainingCourseId = TrainingCourseId,
        OrganizationId = OrganizationId, UserId = UserId,
        PassScore = PassScore, ExpiryDuration = ExpiryDuration
    };
}

public class QuestionDto
{
    public int id { get; set; }
    public bool IsActive { get; set; }
    public int TestId { get; set; }
    public string? Text { get; set; }
    public string? UserId { get; set; }
    public int? CourseSectionId { get; set; }
    public int? sorting { get; set; }
    public List<AnswerDto>? Answers { get; set; }

    public static QuestionDto FromEntity(Question e) => new()
    {
        id = e.id, IsActive = e.IsActive, TestId = e.TestId, Text = e.Text,
        CourseSectionId = e.CourseSectionId, sorting = e.sorting,
        Answers = e.Answers.Select(AnswerDto.FromEntity).ToList()
    };
}

public class AnswerDto
{
    public int id { get; set; }
    public bool IsActive { get; set; }
    public string? Text { get; set; }
    public bool IsCorrect { get; set; }

    public static AnswerDto FromEntity(Answer e) => new()
    {
        id = e.id, IsActive = e.IsActive, Text = e.Text, IsCorrect = e.IsCorrect
    };
}

public class AnswerSubmissionDto
{
    public int QuestionId { get; set; }
    public int SelectedAnswerId { get; set; }
}

public class SubmitTestRequest
{
    public int TestId { get; set; }
    public int? orgId { get; set; }
    public string? UserId { get; set; }
    public List<AnswerSubmissionDto> Answers { get; set; } = [];
    public DateTime ExamDate { get; set; }
}

public class UserTestResultDTO
{
    public int id { get; set; }
    public int TestId { get; set; }
    public TestDto? Test { get; set; }
    public int OrganizationId { get; set; }
    public string? UserId { get; set; }
    public DateTime ExamDate { get; set; }
    public double Score { get; set; }
    public bool IsExpired { get; set; }
    public string? employeeNumber { get; set; }
    public bool IsPassed { get; set; }

    public static UserTestResultDTO FromEntity(UserTestResult e, int passScore) => new()
    {
        id = e.id, TestId = e.TestId, OrganizationId = e.OrganizationId,
        UserId = e.UserId, ExamDate = e.ExamDate, Score = e.Score,
        IsPassed = e.Score >= passScore,
        IsExpired = e.Test != null && (DateTime.Now - e.ExamDate).TotalDays > e.Test.ExpiryDuration
    };
}

// Response wrappers
public class TrainingCoursesResponse : BaseResponse
{
    public List<TrainingCourseDTO> Courses { get; set; } = [];
    public List<CourseSectionDto> CourseSections { get; set; } = [];
    public bool isEditable { get; set; }
    public string? CourseUrl { get; set; }
    public string? SurveyUrl { get; set; }
}

public class CourseSectionResponse : BaseResponse
{
    public List<CourseSectionDto> CourseSections { get; set; } = [];
    public bool isEditable { get; set; }
}

public class TestResponse : BaseResponse
{
    public bool IsEditable { get; set; }
    public TestDto? Test { get; set; }
    public List<CourseSectionDto> sections { get; set; } = [];
    public List<QuestionDto> questions { get; set; } = [];
}

public class CertificateResponse : BaseResponse
{
    public int ResultsId { get; set; }
    public string? CourseName { get; set; }
    public string? StudentName { get; set; }
    public bool Valid { get; set; }
    public int orgId { get; set; }
    public DateTime SubmitDate { get; set; }
}
