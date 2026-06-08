using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TrainingService.Data;
using TrainingService.DTOs;
using TrainingService.Models;
using TrainingService.Models.Shared;

namespace TrainingService.Services;

public class TestServiceImpl : ITestService
{
    private readonly TrainingDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly ICertificateService _certService;
    private readonly ILogger<TestServiceImpl> _logger;

    public TestServiceImpl(TrainingDbContext db, IMemoryCache cache, ICertificateService certService, ILogger<TestServiceImpl> logger)
    {
        _db = db;
        _cache = cache;
        _certService = certService;
        _logger = logger;
    }

    public async Task<BaseResponse> AddUpdateTest(TestDto model)
    {
        var response = new BaseResponse();
        try
        {
            var existing = model.id > 0 ? await _db.Tests.FindAsync(model.id) : null;
            if (existing == null)
            {
                var entity = model.ToEntity();
                _db.Tests.Add(entity);
                await _db.SaveChangesAsync();
                response.IsValid = true; response.Id = entity.id;
                response.Message = "Test added"; response.ArMessage = "تمت إضافة الاختبار";
            }
            else
            {
                existing.Name = model.Name; existing.IsActive = model.IsActive;
                existing.PassScore = model.PassScore; existing.ExpiryDuration = model.ExpiryDuration;
                await _db.SaveChangesAsync();
                response.IsValid = true; response.Id = existing.id;
                response.Message = "Test updated"; response.ArMessage = "تم تحديث الاختبار";
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "AddUpdateTest"); response.IsValid = false; response.Message = ex.InnerException?.Message ?? ex.Message; }
        return response;
    }

    public async Task<TestResponse> GetMyTest(int courseId = 0, string username = "", bool fullAccess = false)
    {
        var response = new TestResponse { IsEditable = fullAccess };
        try
        {
            var test = await _db.Tests
                .Include(t => t.Questions.Where(q => q.IsActive))
                .ThenInclude(q => q.Answers.Where(a => a.IsActive))
                .FirstOrDefaultAsync(t => t.TrainingCourseId == courseId && t.IsActive);

            if (test == null) { response.IsValid = false; response.Message = "Test not found"; return response; }

            response.Test = TestDto.FromEntity(test);
            response.questions = test.Questions.Select(QuestionDto.FromEntity).ToList();

            var sections = await _db.CourseSections
                .Where(s => s.TrainingCourseId == courseId && s.IsActive).ToListAsync();
            response.sections = sections.Select(s => CourseSectionDto.FromEntity(s)).ToList();
            response.IsValid = true;
        }
        catch (Exception ex) { _logger.LogError(ex, "GetMyTest"); response.IsValid = false; response.Message = ex.InnerException?.Message ?? ex.Message; }
        return response;
    }

    public async Task<TestResponse> GetTest(int courseId = 0, string username = "")
    {
        var response = new TestResponse { IsEditable = false };
        try
        {
            var test = await _db.Tests
                .Include(t => t.Questions.Where(q => q.IsActive))
                .ThenInclude(q => q.Answers.Where(a => a.IsActive))
                .FirstOrDefaultAsync(t => t.TrainingCourseId == courseId && t.IsActive);

            if (test == null) { response.IsValid = false; response.Message = "Test not found"; return response; }

            // Hide correct answers for trainee view
            var testDto = TestDto.FromEntity(test);
            testDto.Questions = test.Questions.Select(q => new QuestionDto
            {
                id = q.id, Text = q.Text, IsActive = q.IsActive, TestId = q.TestId,
                CourseSectionId = q.CourseSectionId, sorting = q.sorting,
                Answers = q.Answers.Select(a => new AnswerDto
                {
                    id = a.id, Text = a.Text, IsActive = a.IsActive, IsCorrect = false // hide
                }).ToList()
            }).ToList();

            response.Test = testDto;
            response.IsValid = true;
        }
        catch (Exception ex) { _logger.LogError(ex, "GetTest"); response.IsValid = false; response.Message = ex.InnerException?.Message ?? ex.Message; }
        return response;
    }

    public async Task<BaseResponse> AddUpdateQuestion(QuestionDto model)
    {
        var response = new BaseResponse();
        try
        {
            var existing = model.id > 0 ? await _db.Questions.Include(q => q.Answers).FirstOrDefaultAsync(q => q.id == model.id) : null;
            if (existing == null)
            {
                var q = new Question { Text = model.Text, TestId = model.TestId, IsActive = model.IsActive, CourseSectionId = model.CourseSectionId, sorting = model.sorting };
                if (model.Answers != null)
                    q.Answers = model.Answers.Select(a => new Answer { Text = a.Text, IsCorrect = a.IsCorrect, IsActive = a.IsActive }).ToList();
                _db.Questions.Add(q);
                await _db.SaveChangesAsync();
                response.IsValid = true; response.Id = q.id; response.Message = "Question added";
            }
            else
            {
                existing.Text = model.Text; existing.IsActive = model.IsActive;
                existing.CourseSectionId = model.CourseSectionId; existing.sorting = model.sorting;
                if (model.Answers != null)
                {
                    _db.Answers.RemoveRange(existing.Answers);
                    existing.Answers = model.Answers.Select(a => new Answer { Text = a.Text, IsCorrect = a.IsCorrect, IsActive = a.IsActive }).ToList();
                }
                await _db.SaveChangesAsync();
                response.IsValid = true; response.Id = existing.id; response.Message = "Question updated";
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "AddUpdateQuestion"); response.IsValid = false; response.Message = ex.InnerException?.Message ?? ex.Message; }
        return response;
    }

    public async Task<BaseResponse> DeactivateQuestion(int questionId, string username)
    {
        var q = await _db.Questions.FindAsync(questionId);
        if (q == null) return new BaseResponse { IsValid = false, Message = "Question not found" };
        q.IsActive = false;
        await _db.SaveChangesAsync();
        return new BaseResponse { IsValid = true, Message = "Question deactivated" };
    }

    public async Task<BaseResponse> SubmitTestResult(SubmitTestRequest model)
    {
        var response = new BaseResponse();
        try
        {
            var test = await _db.Tests
                .Include(t => t.Questions.Where(q => q.IsActive))
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(t => t.id == model.TestId);

            if (test == null) return new BaseResponse { IsValid = false, Message = "Test not found" };

            var total = test.Questions.Count;
            var correct = 0;
            var userAnswers = new List<UserAnswer>();

            foreach (var sub in model.Answers)
            {
                var question = test.Questions.FirstOrDefault(q => q.id == sub.QuestionId);
                if (question == null) continue;
                var answer = question.Answers.FirstOrDefault(a => a.id == sub.SelectedAnswerId);
                var isCorrect = answer?.IsCorrect ?? false;
                if (isCorrect) correct++;
                userAnswers.Add(new UserAnswer
                {
                    QuestionId = sub.QuestionId, SelectedAnswerId = sub.SelectedAnswerId, IsCorrect = isCorrect
                });
            }

            var scoreDouble = total > 0 ? (double)correct / total * 100 : 0;
            var score = (int)Math.Round(scoreDouble);          // store as int to match DB column
            var result = new UserTestResult
            {
                TestId = model.TestId, OrganizationId = model.orgId ?? 1,
                UserId = model.UserId, ExamDate = model.ExamDate, Score = score,
                UserAnswers = userAnswers
            };
            _db.UserTestResults.Add(result);
            await _db.SaveChangesAsync();

            // Invalidate both the trainer cache (testId only) and any user-specific caches
            _cache.Remove($"test_results_{model.TestId}");
            if (model.UserId != null)
                _cache.Remove($"test_results_{model.TestId}_{model.UserId}");

            response.IsValid = true; response.Id = result.id;
            response.Message = score >= test.PassScore ? "Passed" : "Failed";
            response.ArMessage = score >= test.PassScore ? "نجحت" : "رسبت";
        }
        catch (Exception ex) { _logger.LogError(ex, "SubmitTestResult"); response.IsValid = false; response.Message = ex.InnerException?.Message ?? ex.Message; }
        return response;
    }

    public async Task<List<UserTestResultDTO>> ViewTestResult(int testId, string username, bool fullAccess = false)
    {
        // Use testId-only key so cache invalidation on submit works for all users
        var cacheKey = fullAccess ? $"test_results_{testId}" : $"test_results_{testId}_{username}";
        if (_cache.TryGetValue(cacheKey, out List<UserTestResultDTO>? cached)) return cached!;

        var query = _db.UserTestResults.Include(r => r.Test).Where(r => r.TestId == testId);
        if (!fullAccess) query = query.Where(r => r.UserId == username);

        var results = await query.OrderByDescending(r => r.ExamDate).ToListAsync();
        var dtos = results.Select(r => UserTestResultDTO.FromEntity(r, r.Test?.PassScore ?? 60)).ToList();

        _cache.Set(cacheKey, dtos, TimeSpan.FromMinutes(5));
        return dtos;
    }

    public async Task<CertificateResponse> PrintCertificate(int resultId, string username, bool fullAccess = false)
    {
        var response = new CertificateResponse();
        try
        {
            var result = await _db.UserTestResults
                .Include(r => r.Test).ThenInclude(t => t!.TrainingCourse)
                .FirstOrDefaultAsync(r => r.id == resultId);

            if (result == null || (!fullAccess && result.UserId != username))
            {
                response.IsValid = false; response.Message = "Result not found";
                return response;
            }

            response.IsValid = true;
            response.ResultsId = result.id;
            response.CourseName = result.Test?.TrainingCourse?.Name ?? "";
            response.StudentName = username;
            response.orgId = result.OrganizationId;
            response.SubmitDate = result.ExamDate;
            response.Valid = result.Test != null && (DateTime.Now - result.ExamDate).TotalDays <= result.Test.ExpiryDuration;
        }
        catch (Exception ex) { _logger.LogError(ex, "PrintCertificate"); response.IsValid = false; response.Message = ex.InnerException?.Message ?? ex.Message; }
        return response;
    }
}
