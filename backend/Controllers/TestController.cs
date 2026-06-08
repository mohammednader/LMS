using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrainingService.DTOs;
using TrainingService.Services;

namespace TrainingService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ITestService _test;
    private readonly ICertificateService _cert;

    public TestController(ITestService test, ICertificateService cert)
    {
        _test = test;
        _cert = cert;
    }

    private string Username => User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
    private int OrgId => int.TryParse(User.FindFirstValue("orgId"), out var id) ? id : 0;
    private bool IsTrainer => User.IsInRole("Training") || User.IsInRole("Developer");

    [HttpPost("AddUpdateTest")]
    public async Task<IActionResult> AddUpdateTest([FromBody] TestDto model)
    {
        model.UserId = Username;
        model.OrganizationId = OrgId;
        return Ok(await _test.AddUpdateTest(model));
    }

    [HttpPost("AddUpdateQuestion")]
    public async Task<IActionResult> AddUpdateQuestion([FromBody] QuestionDto model)
    {
        return Ok(await _test.AddUpdateQuestion(model));
    }

    [HttpDelete("Question/{questionId}")]
    public async Task<IActionResult> DeactivateQuestion(int questionId)
    {
        return Ok(await _test.DeactivateQuestion(questionId, Username));
    }

    // Trainee test view (hides correct answers)
    [HttpGet("Test/{courseId}")]
    public async Task<IActionResult> GetTest(int courseId)
    {
        return Ok(await _test.GetTest(courseId, Username));
    }

    // Trainer test view (shows full test with answers)
    [HttpGet("MyTest/{courseId}")]
    public async Task<IActionResult> GetMyTest(int courseId)
    {
        return Ok(await _test.GetMyTest(courseId, Username, IsTrainer));
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitTestRequest model)
    {
        model.UserId ??= Username;
        model.orgId ??= OrgId;
        model.ExamDate = DateTime.Now;
        return Ok(await _test.SubmitTestResult(model));
    }

    [HttpGet("TestResults/{testId}")]
    public async Task<IActionResult> TestResults(int testId)
    {
        return Ok(await _test.ViewTestResult(testId, Username, IsTrainer));
    }

    [HttpGet("GenerateCertificate/{resultId}")]
    public async Task<IActionResult> GenerateCertificate(int resultId)
    {
        var cert = await _test.PrintCertificate(resultId, Username, IsTrainer);
        if (!cert.IsValid) return BadRequest(cert);

        var pdfBytes = _cert.GenerateCertificate(cert.orgId, cert.StudentName!, cert.CourseName!, cert.SubmitDate, cert.Valid);
        if (pdfBytes.Length == 0) return StatusCode(500, "Failed to generate certificate");

        return File(pdfBytes, "application/pdf", $"Certificate_{resultId}.pdf");
    }
}
