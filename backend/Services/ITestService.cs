using TrainingService.DTOs;
using TrainingService.Models.Shared;

namespace TrainingService.Services;

public interface ITestService
{
    Task<BaseResponse> AddUpdateTest(TestDto model);
    Task<TestResponse> GetMyTest(int courseId = 0, string username = "", bool fullAccess = false);
    Task<TestResponse> GetTest(int courseId = 0, string username = "");
    Task<BaseResponse> AddUpdateQuestion(QuestionDto model);
    Task<BaseResponse> DeactivateQuestion(int questionId, string username);
    Task<BaseResponse> SubmitTestResult(SubmitTestRequest model);
    Task<List<UserTestResultDTO>> ViewTestResult(int testId, string username, bool fullAccess = false);
    Task<CertificateResponse> PrintCertificate(int resultId, string username, bool fullAccess = false);
}
