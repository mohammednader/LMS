using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TrainingService.DTOs;
using TrainingService.Services;

namespace TrainingService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CourseController : ControllerBase
{
    private readonly ITrainingService _training;

    public CourseController(ITrainingService training) => _training = training;

    private string Username => User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
    private int OrgId => int.TryParse(User.FindFirstValue("orgId"), out var id) ? id : 0;
    private bool IsTrainer => User.IsInRole("Training") || User.IsInRole("Developer");

    // Trainee: view available courses
    [HttpGet("Courses")]
    public async Task<IActionResult> GetCourses()
    {
        var result = await _training.GetTrainingCourses(OrgId, Username);
        return Ok(result);
    }

    [HttpGet("CourseDetails/{courseId}")]
    public async Task<IActionResult> GetCourseDetails(int courseId)
    {
        var result = await _training.GetTrainingCourses(OrgId, Username, courseId);
        return Ok(result);
    }

    // Trainer/Manager: view own courses
    [HttpGet("MyCourses")]
    public async Task<IActionResult> GetMyCourses()
    {
        var result = await _training.GetTrainingCourses(OrgId, Username, fullAccess: IsTrainer);
        return Ok(result);
    }

    [HttpGet("MyCourseDetails/{courseId}")]
    public async Task<IActionResult> GetMyCourseDetails(int courseId)
    {
        var result = await _training.GetTrainingCourses(OrgId, Username, courseId, fullAccess: IsTrainer);
        return Ok(result);
    }

    [HttpGet("MyCourseSections/{courseId}")]
    public async Task<IActionResult> GetMyCourseSections(int courseId)
    {
        var result = await _training.GetCourseSections(OrgId, Username, courseId);
        return Ok(result);
    }

    [HttpPost("AddUpdateCourse")]
    public async Task<IActionResult> AddUpdateCourse([FromBody] TrainingCourseDTO model)
    {
        model.OrganizationId = OrgId;
        var result = await _training.AddUpdateTrainingCourse(Username, model);
        return Ok(result);
    }

    [HttpPost("AddUpdateSection")]
    public async Task<IActionResult> AddUpdateSection([FromBody] CourseSectionDto model)
    {
        model.OrganizationId = OrgId;
        var result = await _training.AddUpdateCourseSection(Username, model);
        return Ok(result);
    }

    [HttpDelete("Section/{id}")]
    public async Task<IActionResult> DeleteSection(int id)
    {
        var result = await _training.DeleteSection(id);
        return Ok(result);
    }
}
