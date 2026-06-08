using TrainingService.DTOs;
using TrainingService.Models.Shared;

namespace TrainingService.Services;

public interface ITrainingService
{
    Task<BaseResponse> AddUpdateTrainingCourse(string userName, TrainingCourseDTO model);
    Task<BaseResponse> AddUpdateCourseSection(string userName, CourseSectionDto model);
    Task<TrainingCoursesResponse> GetTrainingCourses(int orgId = 0, string username = "", int courseId = 0, bool fullAccess = false);
    Task<CourseSectionResponse> GetCourseSections(int orgId = 0, string username = "", int courseId = 0);
    Task<BaseResponse> DeleteSection(int id);
}
