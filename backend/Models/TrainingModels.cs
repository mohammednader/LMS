namespace TrainingService.Models;

public class FileRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string StoredPath { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.Now;
}

public class TrainingCourse
{
    public int id { get; set; }
    public string? Name { get; set; }
    public string? NameAr { get; set; }
    public DateTime Creatd { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
    public int OrganizationId { get; set; }
    public string? UserId { get; set; }
    public string? TargetedAudiance { get; set; }
    public Guid? AttachRecordId { get; set; }
    public int? SurveyId { get; set; }

    public ICollection<CourseSection> Sections { get; set; } = [];
    public ICollection<Test> Tests { get; set; } = [];
}

public class CourseSection
{
    public int id { get; set; }
    public int TrainingCourseId { get; set; }
    public string? NameAr { get; set; }
    public string? Name { get; set; }
    public DateTime Creatd { get; set; } = DateTime.Now;
    public bool IsActive { get; set; } = true;
    public int OrganizationId { get; set; }
    public string? UserId { get; set; }
    public Guid? AttachRecordId { get; set; }   // uploaded file
    public string? ExternalUrl { get; set; }     // link to YouTube / SharePoint / etc.
    public string? Description { get; set; }

    public TrainingCourse? TrainingCourse { get; set; }
}

public class Test
{
    public int id { get; set; }
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
    public string? UserId { get; set; }
    public int TrainingCourseId { get; set; }
    public int OrganizationId { get; set; }
    public int PassScore { get; set; } = 60;
    public int ExpiryDuration { get; set; } = 365;

    public TrainingCourse? TrainingCourse { get; set; }
    public ICollection<Question> Questions { get; set; } = [];
}

public class Question
{
    public int id { get; set; }
    public string? Text { get; set; }
    public int TestId { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CourseSectionId { get; set; }
    public int? sorting { get; set; }

    public Test? Test { get; set; }
    public ICollection<Answer> Answers { get; set; } = [];
}

public class Answer
{
    public int id { get; set; }
    public string? Text { get; set; }
    public bool IsCorrect { get; set; }
    public bool IsActive { get; set; } = true;
    public int QuestionId { get; set; }

    public Question? Question { get; set; }
}

public class UserTestResult
{
    public int id { get; set; }
    public int TestId { get; set; }
    public int OrganizationId { get; set; }
    public string? UserId { get; set; }
    public DateTime ExamDate { get; set; }
    public int Score { get; set; }

    public Test? Test { get; set; }
    public ICollection<UserAnswer> UserAnswers { get; set; } = [];
}

public class UserAnswer
{
    public int id { get; set; }
    public int UserTestResultId { get; set; }
    public int QuestionId { get; set; }
    public int SelectedAnswerId { get; set; }
    public bool IsCorrect { get; set; }

    public UserTestResult? UserTestResult { get; set; }
    public Question? Question { get; set; }
    public Answer? SelectedAnswer { get; set; }
}
